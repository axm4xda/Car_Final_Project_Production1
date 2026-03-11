using Car_Project.Models;

namespace Car_Project.Services.Abstractions
{
    public interface INotificationService
    {
        /// <summary>Yeni bildiriş yaradır.</summary>
        Task<Notification> CreateAsync(Notification notification);

        /// <summary>Müəyyən istifadəçiyə bildiriş göndərir.</summary>
        Task<Notification> CreateForUserAsync(string userId, string title, string message, NotificationType type, string? link = null);

        /// <summary>Admin / SuperAdmin istifadəçilərinə bildiriş göndərir (UserId = null).</summary>
        Task<Notification> CreateForAdminsAsync(string title, string message, NotificationType type, string? link = null);

        /// <summary>İstifadəçinin bildirişlərini qaytarır (ən yenidən köhnəyə).</summary>
        Task<IList<Notification>> GetByUserIdAsync(string userId);

        /// <summary>Admin bildirişlərini qaytarır (UserId == null olanlar).</summary>
        Task<IList<Notification>> GetAdminNotificationsAsync();

        /// <summary>İstifadəçinin oxunmamış bildiriş sayını qaytarır.</summary>
        Task<int> GetUnreadCountAsync(string userId);

        /// <summary>Admin oxunmamış bildiriş sayını qaytarır.</summary>
        Task<int> GetAdminUnreadCountAsync();

        /// <summary>Bildirişi oxunmuş olaraq işarələyir.</summary>
        Task MarkAsReadAsync(int id);

        /// <summary>İstifadəçinin bütün bildirişlərini oxunmuş olaraq işarələyir.</summary>
        Task MarkAllAsReadAsync(string userId);

        /// <summary>Admin bildirişlərini oxunmuş olaraq işarələyir.</summary>
        Task MarkAllAdminAsReadAsync();

        /// <summary>Bildirişi silir.</summary>
        Task DeleteAsync(int id);

        /// <summary>Id-yə görə bildiriş qaytarır.</summary>
        Task<Notification?> GetByIdAsync(int id);
    }
}
