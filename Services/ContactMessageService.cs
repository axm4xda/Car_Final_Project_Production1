using Car_Project.Data;
using Car_Project.Models;
using Car_Project.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Car_Project.Services
{
    public class ContactMessageService : IContactMessageService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public ContactMessageService(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // — PUBLIC —

        public async Task<ContactMessage> SendAsync(ContactMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            message.IsRead      = false;
            message.CreatedDate = DateTime.UtcNow;

            await _context.ContactMessages.AddAsync(message);
            await _context.SaveChangesAsync();

            // Adminlərə yeni əlaqə mesajı bildirişi göndər
            await _notificationService.CreateForAdminsAsync(
                "Yeni Əlaqə Mesajı",
                $"{message.FullName} tərəfindən yeni mesaj: \"{(message.Subject?.Length > 50 ? message.Subject[..50] + "…" : message.Subject ?? "Mövzusuz")}\"",
                NotificationType.NewContactMessage,
                $"/Admin/ContactMessage/Details/{message.Id}");

            return message;
        }

        // — ADMIN —

        public async Task<IList<ContactMessage>> GetAllAdminAsync()
        {
            return await _context.ContactMessages
                .AsNoTracking()
                .OrderByDescending(m => m.CreatedDate)
                .ToListAsync();
        }

        public async Task<IList<ContactMessage>> GetUnreadAsync()
        {
            return await _context.ContactMessages
                .AsNoTracking()
                .Where(m => !m.IsRead)
                .OrderByDescending(m => m.CreatedDate)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync()
        {
            return await _context.ContactMessages
                .CountAsync(m => !m.IsRead);
        }

        public async Task<ContactMessage?> GetByIdAsync(int id)
        {
            return await _context.ContactMessages
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task MarkAsReadAsync(int id)
        {
            var message = await _context.ContactMessages.FindAsync(id)
                ?? throw new KeyNotFoundException($"Id={id} olan mesaj tapılmadı.");

            message.IsRead = true;
            await _context.SaveChangesAsync();
        }

        public async Task MarkAllAsReadAsync()
        {
            var unread = await _context.ContactMessages
                .Where(m => !m.IsRead)
                .ToListAsync();

            foreach (var message in unread)
                message.IsRead = true;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var message = await _context.ContactMessages.FindAsync(id)
                ?? throw new KeyNotFoundException($"Id={id} olan mesaj tapılmadı.");

            _context.ContactMessages.Remove(message);
            await _context.SaveChangesAsync();
        }
    }
}
