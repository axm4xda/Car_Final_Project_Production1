using Car_Project.Services.Abstractions;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace Car_Project.Hubs
{
    /// <summary>
    /// AI Chatbot SignalR Hub — hər istifadəçi üçün sessiya bazasında söhbət tarixçəsi saxlayır.
    /// Anonim istifadəçilər də istifadə edə bilər.
    /// </summary>
    public sealed class AiChatHub : Hub
    {
        private readonly IAiChatService _aiService;

        // Thread-safe: ConcurrentDictionary + lock-free conversation storage
        private static readonly ConcurrentDictionary<string, ConversationState> _conversations = new();

        // Maksimum mesaj uzunluğu
        private const int MaxMessageLength = 1000;
        // Tarixçə limiti
        private const int MaxHistorySize = 20;
        // Rate limit: minimum interval (saniyə) ardıcıl mesajlar arasında
        private const int RateLimitSeconds = 2;

        public AiChatHub(IAiChatService aiService)
        {
            _aiService = aiService;
        }

        /// <summary>
        /// İstifadəçi mesajını qəbul edir, AI-dan cavab alır, hər ikisini caller-a göndərir.
        /// </summary>
        public async Task SendMessage(string userMessage)
        {
            if (string.IsNullOrWhiteSpace(userMessage)) return;

            // Input sanitizasiyası: uzunluğu məhdudlaşdır
            userMessage = userMessage.Trim();
            if (userMessage.Length > MaxMessageLength)
            {
                userMessage = userMessage[..MaxMessageLength];
            }

            var connId = Context.ConnectionId;
            var state = _conversations.GetOrAdd(connId, _ => new ConversationState());

            // Rate limiting: çox tez-tez mesaj göndərilməsinin qarşısını al
            var now = DateTime.UtcNow;
            if ((now - state.LastMessageTime).TotalSeconds < RateLimitSeconds)
            {
                await Clients.Caller.SendAsync("AiResponse",
                    "Çox tez mesaj göndərirsiniz. Zəhmət olmasa bir az gözləyin.");
                return;
            }

            // Əgər hələ cavab gözlənilirsə, yeni mesaj qəbul etmə
            if (state.IsBusy)
            {
                await Clients.Caller.SendAsync("AiResponse",
                    "Əvvəlki mesajınızın cavabı gözlənilir. Zəhmət olmasa gözləyin.");
                return;
            }

            state.LastMessageTime = now;
            state.IsBusy = true;

            // İstifadəçi mesajını tarixçəyə əlavə et (lock ilə list-ə safe giriş)
            List<AiChatMessage> historyCopy;
            lock (state.Lock)
            {
                state.History.Add(new AiChatMessage { Role = "user", Content = userMessage });

                // Tarixçəni məhdudlaşdır (yaddaş optimizasiyası)
                if (state.History.Count > MaxHistorySize)
                {
                    state.History.RemoveRange(0, state.History.Count - MaxHistorySize);
                }

                // AI servisə göndərmək üçün tarixçanın kopiyasını al
                historyCopy = new List<AiChatMessage>(state.History);
            }

            // Typing indikator göndər
            await Clients.Caller.SendAsync("AiTyping", true);

            try
            {
                // CancellationToken — bağlantı kəsilərsə əməliyyatı dayandır
                var ct = Context.ConnectionAborted;
                var aiResponse = await _aiService.GetResponseAsync(historyCopy, ct);

                // AI cavabını tarixçəyə əlavə et
                lock (state.Lock)
                {
                    state.History.Add(new AiChatMessage { Role = "assistant", Content = aiResponse });
                }

                // Cavabı caller-a göndər
                await Clients.Caller.SendAsync("AiResponse", aiResponse);
            }
            catch (OperationCanceledException)
            {
                // Bağlantı kəsildi — SendAsync çağırmaq mümkün deyil, birbaşa çıx
                return;
            }
            catch (Exception)
            {
                try
                {
                    await Clients.Caller.SendAsync("AiResponse",
                        "Üzr istəyirəm, texniki xəta baş verdi. Zəhmət olmasa bir az sonra yenidən cəhd edin.");
                }
                catch
                {
                    // Bağlantı artıq kəsilib — ignore
                }
            }
            finally
            {
                state.IsBusy = false;

                // Typing-i yalnız bağlantı hələ aktiv olduqda söndür
                if (!Context.ConnectionAborted.IsCancellationRequested)
                {
                    try
                    {
                        await Clients.Caller.SendAsync("AiTyping", false);
                    }
                    catch
                    {
                        // Bağlantı kəsilib — ignore
                    }
                }
            }
        }

        /// <summary>
        /// Söhbət tarixçəsini təmizləyir.
        /// </summary>
        public async Task ClearHistory()
        {
            _conversations.TryRemove(Context.ConnectionId, out _);
            await Clients.Caller.SendAsync("HistoryCleared");
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _conversations.TryRemove(Context.ConnectionId, out _);
            return base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Hər bağlantı üçün söhbət vəziyyəti: tarixçə + rate limit timestamp + busy flag.
        /// </summary>
        private sealed class ConversationState
        {
            public readonly object Lock = new();
            public readonly List<AiChatMessage> History = new();
            public DateTime LastMessageTime = DateTime.MinValue;
            public volatile bool IsBusy;
        }
    }
}
