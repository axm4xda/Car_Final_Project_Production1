using Car_Project.Data;
using Car_Project.Models;
using Car_Project.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Car_Project.Hubs
{
    // DTO for the conversation list sent to the client
    internal sealed class ChatUserDto
    {
        public string Id          { get; set; } = "";
        public string Name        { get; set; } = "";
        public string Role        { get; set; } = "";
        public bool   Online      { get; set; }
        public string LastMessage { get; set; } = "";
        public string LastTime    { get; set; } = "";
        public DateTime LastSentAt { get; set; }
        public int    UnreadCount { get; set; }
    }

    [Authorize]
    public class ChatHub : Hub
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _db;
        private readonly INotificationService _notificationService;

        // userId → connectionId mapping (in-memory; fine for single-server)
        private static readonly Dictionary<string, string> _connections = new();

        public ChatHub(
            UserManager<AppUser> userManager,
            ApplicationDbContext db,
            INotificationService notificationService)
        {
            _userManager = userManager;
            _db = db;
            _notificationService = notificationService;
        }

        // ─────────────────────────────────────────────
        //  Connection lifecycle
        // ─────────────────────────────────────────────

        public override async Task OnConnectedAsync()
        {
            var user = await _userManager.GetUserAsync(Context.User!);
            if (user != null)
            {
                lock (_connections)
                    _connections[user.Id] = Context.ConnectionId;

                await Clients.All.SendAsync("UserOnline", user.Id, user.FullName);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var user = await _userManager.GetUserAsync(Context.User!);
            if (user != null)
            {
                lock (_connections)
                    _connections.Remove(user.Id);

                await Clients.All.SendAsync("UserOffline", user.Id);
            }
            await base.OnDisconnectedAsync(exception);
        }

        // ─────────────────────────────────────────────
        //  Send a private message
        // ─────────────────────────────────────────────

        public async Task SendPrivateMessage(string receiverId, string messageText)
        {
            var sender = await _userManager.GetUserAsync(Context.User!);
            if (sender == null || string.IsNullOrWhiteSpace(messageText)) return;

            var receiver = await _userManager.FindByIdAsync(receiverId);
            if (receiver == null) return;

            // Persist
            var msg = new ChatMessage
            {
                SenderId   = sender.Id,
                SenderName = sender.FullName,
                ReceiverId = receiverId,
                Message    = messageText.Trim(),
                SentAt     = DateTime.UtcNow,
                IsRead     = false,
                ReadAt     = null
            };
            _db.ChatMessages.Add(msg);
            await _db.SaveChangesAsync();

            // Count unread for receiver from this sender
            var unreadCount = await _db.ChatMessages
                .CountAsync(m => m.SenderId == sender.Id &&
                                 m.ReceiverId == receiverId &&
                                 !m.IsRead);

            var payload = new
            {
                id           = msg.Id,
                senderId     = sender.Id,
                senderName   = sender.FullName,
                receiverId   = receiverId,
                message      = msg.Message,
                sentAt       = msg.SentAt.ToString("HH:mm"),
                isRead       = false,
                readAt       = (string?)null
            };

            // Find receiver's connection
            string? receiverConn;
            lock (_connections)
                _connections.TryGetValue(receiverId, out receiverConn);

            if (receiverConn != null)
            {
                // Deliver the message in real time
                await Clients.Client(receiverConn).SendAsync("ReceiveMessage", payload);

                // Tell receiver to bump the unread counter for this conversation
                await Clients.Client(receiverConn).SendAsync("UpdateUnreadCount", sender.Id, unreadCount);

                // Tell receiver to reorder the conversation list
                await Clients.Client(receiverConn).SendAsync("ConversationBumped", sender.Id, sender.FullName, msg.Message, msg.SentAt.ToString("HH:mm"));
            }

            // Echo back to sender (so their own message appears instantly)
            await Clients.Caller.SendAsync("ReceiveMessage", payload);

            // Push SignalR notification to receiver if offline or in a different tab
            await _notificationService.CreateForUserAsync(
                receiverId,
                "Yeni Mesaj",
                $"{sender.FullName}: {(msg.Message.Length > 60 ? msg.Message[..60] + "…" : msg.Message)}",
                NotificationType.NewChatMessage,
                "/Message/Index");
        }

        // ─────────────────────────────────────────────
        //  Mark messages as read (called when opening a conversation)
        // ─────────────────────────────────────────────

        public async Task MarkAsRead(string senderId)
        {
            var me = await _userManager.GetUserAsync(Context.User!);
            if (me == null) return;

            var unread = await _db.ChatMessages
                .Where(m => m.SenderId == senderId &&
                            m.ReceiverId == me.Id  &&
                            !m.IsRead)
                .ToListAsync();

            if (!unread.Any()) return;

            var now = DateTime.UtcNow;
            var ids = new List<int>();
            foreach (var m in unread)
            {
                m.IsRead = true;
                m.ReadAt = now;
                ids.Add(m.Id);
            }
            await _db.SaveChangesAsync();

            // Tell the sender their messages have been read (double blue ticks)
            string? senderConn;
            lock (_connections)
                _connections.TryGetValue(senderId, out senderConn);

            if (senderConn != null)
                await Clients.Client(senderConn).SendAsync("MessagesRead", me.Id, ids);

            // Reset unread counter for caller
            await Clients.Caller.SendAsync("UpdateUnreadCount", senderId, 0);
        }

        // ─────────────────────────────────────────────
        //  Get the conversation list (with last message + unread count)
        // ─────────────────────────────────────────────

        public async Task GetChatUsers()
        {
            var me = await _userManager.GetUserAsync(Context.User!);
            if (me == null) return;

            var allUsers = await _userManager.Users
                .Where(u => u.Id != me.Id)
                .ToListAsync();

            // Pre-load all relevant messages in one query
            var allUserIds = allUsers.Select(u => u.Id).ToList();
            var allMessages = await _db.ChatMessages
                .Where(m =>
                    (m.SenderId == me.Id    && m.ReceiverId != null && allUserIds.Contains(m.ReceiverId!)) ||
                    (m.ReceiverId == me.Id  && allUserIds.Contains(m.SenderId)))
                .ToListAsync();

            var result = new List<ChatUserDto>();

            foreach (var u in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(u);
                if (!roles.Any(r => new[] { "SuperAdmin", "Admin", "Agent", "User" }.Contains(r)))
                    continue;

                bool online;
                lock (_connections)
                    online = _connections.ContainsKey(u.Id);

                var conv = allMessages
                    .Where(m =>
                        (m.SenderId == me.Id && m.ReceiverId == u.Id) ||
                        (m.SenderId == u.Id  && m.ReceiverId == me.Id))
                    .OrderByDescending(m => m.SentAt)
                    .FirstOrDefault();

                var unread = allMessages
                    .Count(m => m.SenderId == u.Id &&
                                m.ReceiverId == me.Id &&
                                !m.IsRead);

                result.Add(new ChatUserDto
                {
                    Id          = u.Id,
                    Name        = u.FullName ?? u.UserName ?? "",
                    Role        = roles.FirstOrDefault() ?? "User",
                    Online      = online,
                    LastMessage = conv?.Message ?? "",
                    LastTime    = conv?.SentAt.ToString("HH:mm") ?? "",
                    LastSentAt  = conv?.SentAt ?? DateTime.MinValue,
                    UnreadCount = unread
                });
            }

            var sorted = result
                .OrderByDescending(x => x.LastSentAt)
                .ThenBy(x => x.Name)
                .ToList();

            await Clients.Caller.SendAsync("ChatUserList", sorted);
        }

        // ─────────────────────────────────────────────
        //  Load chat history between caller and another user
        // ─────────────────────────────────────────────

        public async Task LoadHistory(string otherUserId)
        {
            var me = await _userManager.GetUserAsync(Context.User!);
            if (me == null) return;

            var history = await _db.ChatMessages
                .Where(m =>
                    (m.SenderId == me.Id        && m.ReceiverId == otherUserId) ||
                    (m.SenderId == otherUserId  && m.ReceiverId == me.Id))
                .OrderBy(m => m.SentAt)
                .Select(m => new
                {
                    id         = m.Id,
                    senderId   = m.SenderId,
                    senderName = m.SenderName,
                    message    = m.Message,
                    sentAt     = m.SentAt.ToString("HH:mm"),
                    isRead     = m.IsRead,
                    readAt     = m.ReadAt != null ? m.ReadAt.Value.ToString("HH:mm") : (string?)null,
                    isMe       = m.SenderId == me.Id
                })
                .ToListAsync();

            await Clients.Caller.SendAsync("ChatHistory", history);

            // Auto-mark messages from otherUser as read now that the chat is open
            await MarkAsRead(otherUserId);
        }

        // ─────────────────────────────────────────────
        //  Get total unread count (for notification badge in nav)
        // ─────────────────────────────────────────────

        public async Task GetTotalUnread()
        {
            var me = await _userManager.GetUserAsync(Context.User!);
            if (me == null) return;

            var total = await _db.ChatMessages
                .CountAsync(m => m.ReceiverId == me.Id && !m.IsRead);

            await Clients.Caller.SendAsync("TotalUnread", total);
        }
    }
}
