using Car_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace Car_Project.Hubs
{
    /// <summary>
    /// Real-time notification hub.
    /// Authenticated users join a personal group (userId) on connect so that
    /// the server can push targeted notifications at any time via
    ///   _hubContext.Clients.Group(userId).SendAsync(...)
    /// </summary>
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly UserManager<AppUser> _userManager;

        // ── Tracks how many connections each user has open (tab count).
        //    Lets us know whether to count the user as "online".
        private static readonly Dictionary<string, int> _connectionCounts = new();

        public NotificationHub(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        // ─────────────────────────────────────────────────────────────
        //  Lifecycle
        // ─────────────────────────────────────────────────────────────

        public override async Task OnConnectedAsync()
        {
            var user = await _userManager.GetUserAsync(Context.User!);
            if (user != null)
            {
                // Each user has their own SignalR group named after their ID.
                // This lets us push to ALL tabs the user has open in one call.
                await Groups.AddToGroupAsync(Context.ConnectionId, user.Id);

                lock (_connectionCounts)
                    _connectionCounts[user.Id] =
                        (_connectionCounts.TryGetValue(user.Id, out var c) ? c : 0) + 1;
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var user = await _userManager.GetUserAsync(Context.User!);
            if (user != null)
            {
                lock (_connectionCounts)
                {
                    if (_connectionCounts.TryGetValue(user.Id, out var c))
                    {
                        if (c <= 1) _connectionCounts.Remove(user.Id);
                        else        _connectionCounts[user.Id] = c - 1;
                    }
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        // ─────────────────────────────────────────────────────────────
        //  Client-callable methods
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Client calls this to get the current unread count on (re-)connect.
        /// The server responds by invoking "UnreadCount" back on the caller.
        /// </summary>
        public async Task RequestUnreadCount()
        {
            var user = await _userManager.GetUserAsync(Context.User!);
            if (user == null) return;

            await Clients.Caller.SendAsync("UnreadCount", 0 /* resolved by controller */);
        }
    }
}
