namespace Car_Project.Services.Abstractions
{
    public interface IAiChatService
    {
        /// <summary>
        /// İstifadəçi mesajını AI-a göndərir və cavabı qaytarır.
        /// conversationHistory: əvvəlki mesajlar (role + content cütləri).
        /// </summary>
        Task<string> GetResponseAsync(List<AiChatMessage> conversationHistory, CancellationToken ct = default);

        /// <summary>
        /// Verilənlər bazasından avtomobil inventarının xülasəsini hazırlayır.
        /// Bu xülasə sistem mesajı olaraq AI-a göndərilir ki, real data əsasında cavab versin.
        /// </summary>
        Task<string> GetCarInventorySummaryAsync(CancellationToken ct = default);
    }

    public sealed class AiChatMessage
    {
        public string Role    { get; set; } = "user";   // "system", "user", "assistant"
        public string Content { get; set; } = string.Empty;
    }
}
