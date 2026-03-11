using Car_Project.Data;
using Car_Project.Models;
using Car_Project.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Car_Project.Services
{
    public class ReviewService : IReviewService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public ReviewService(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<IList<Review>> GetApprovedAsync()
        {
            return await _context.Reviews
                .AsNoTracking()
                .Where(r => r.IsApproved)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
        }

        public async Task<IList<Review>> GetTopRatedAsync(int count = 6)
        {
            return await _context.Reviews
                .AsNoTracking()
                .Where(r => r.IsApproved)
                .OrderByDescending(r => r.Rating)
                .ThenByDescending(r => r.CreatedDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IList<Review>> GetAllAdminAsync()
        {
            return await _context.Reviews
                .AsNoTracking()
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
        }

        public async Task<Review?> GetByIdAsync(int id)
        {
            return await _context.Reviews
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Review> CreateAsync(Review review)
        {
            if (review == null) throw new ArgumentNullException(nameof(review));

            if (review.Rating < 1 || review.Rating > 5)
                throw new ArgumentOutOfRangeException(nameof(review.Rating), "Reytinq 1–5 arasında olmalıdır.");

            review.IsApproved  = false;
            review.CreatedDate = DateTime.UtcNow;

            await _context.Reviews.AddAsync(review);
            await _context.SaveChangesAsync();

            // Adminlərə yeni rəy bildirişi göndər
            await _notificationService.CreateForAdminsAsync(
                "Yeni Müştəri Rəyi",
                $"{review.AuthorName} tərəfindən yeni {review.Rating}⭐ rəy əlavə edildi. Təsdiq gözləyir.",
                NotificationType.NewReview,
                "/Admin/Review/Index");

            return review;
        }

        public async Task UpdateAsync(Review review)
        {
            if (review == null) throw new ArgumentNullException(nameof(review));

            var existing = await _context.Reviews.FindAsync(review.Id)
                ?? throw new KeyNotFoundException($"Id={review.Id} olan rəy tapılmadı.");

            if (review.Rating < 1 || review.Rating > 5)
                throw new ArgumentOutOfRangeException(nameof(review.Rating), "Reytinq 1–5 arasında olmalıdır.");

            existing.AuthorName  = review.AuthorName;
            existing.AuthorTitle = review.AuthorTitle;
            existing.AvatarUrl   = review.AvatarUrl;
            existing.Content     = review.Content;
            existing.Rating      = review.Rating;
            existing.IsApproved  = review.IsApproved;

            await _context.SaveChangesAsync();
        }

        public async Task ApproveAsync(int id)
        {
            var review = await _context.Reviews.FindAsync(id)
                ?? throw new KeyNotFoundException($"Id={id} olan rəy tapılmadı.");

            review.IsApproved = true;
            await _context.SaveChangesAsync();
        }

        public async Task RejectAsync(int id)
        {
            var review = await _context.Reviews.FindAsync(id)
                ?? throw new KeyNotFoundException($"Id={id} olan rəy tapılmadı.");

            review.IsApproved = false;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var review = await _context.Reviews.FindAsync(id)
                ?? throw new KeyNotFoundException($"Id={id} olan rəy tapılmadı.");

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
        }
    }
}
