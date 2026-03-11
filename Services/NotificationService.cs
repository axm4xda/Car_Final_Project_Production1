using Car_Project.Data;
using Car_Project.Models;
using Car_Project.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Car_Project.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Notification> CreateAsync(Notification notification)
        {
            notification.CreatedDate = DateTime.UtcNow;
            notification.IsRead = false;

            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task<Notification> CreateForUserAsync(string userId, string title, string message, NotificationType type, string? link = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                Link = link,
                IsRead = false,
                CreatedDate = DateTime.UtcNow
            };

            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task<Notification> CreateForAdminsAsync(string title, string message, NotificationType type, string? link = null)
        {
            var notification = new Notification
            {
                UserId = null,
                Title = title,
                Message = message,
                Type = type,
                Link = link,
                IsRead = false,
                CreatedDate = DateTime.UtcNow
            };

            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task<IList<Notification>> GetByUserIdAsync(string userId)
        {
            return await _context.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();
        }

        public async Task<IList<Notification>> GetAdminNotificationsAsync()
        {
            return await _context.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == null)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task<int> GetAdminUnreadCountAsync()
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == null && !n.IsRead);
        }

        public async Task MarkAsReadAsync(int id)
        {
            var notification = await _context.Notifications.FindAsync(id)
                ?? throw new KeyNotFoundException($"Id={id} olan bildiriş tapılmadı.");

            notification.IsRead = true;
            await _context.SaveChangesAsync();
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            var unread = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var n in unread)
                n.IsRead = true;

            await _context.SaveChangesAsync();
        }

        public async Task MarkAllAdminAsReadAsync()
        {
            var unread = await _context.Notifications
                .Where(n => n.UserId == null && !n.IsRead)
                .ToListAsync();

            foreach (var n in unread)
                n.IsRead = true;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var notification = await _context.Notifications.FindAsync(id)
                ?? throw new KeyNotFoundException($"Id={id} olan bildiriş tapılmadı.");

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<Notification?> GetByIdAsync(int id)
        {
            return await _context.Notifications
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.Id == id);
        }
    }
}
