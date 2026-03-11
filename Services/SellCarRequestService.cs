using Car_Project.Data;
using Car_Project.Models;
using Car_Project.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Car_Project.Services
{
    public class SellCarRequestService : ISellCarRequestService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public SellCarRequestService(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // ?? PUBLIC ????????????????????????????????????????????????????????????

        public async Task<SellCarRequest> SubmitAsync(SellCarRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            request.IsReviewed  = false;
            request.CreatedDate = DateTime.UtcNow;

            await _context.SellCarRequests.AddAsync(request);
            await _context.SaveChangesAsync();

            // Adminlərə yeni satış müraciəti bildirişi göndər
            await _notificationService.CreateForAdminsAsync(
                "Yeni Satış Müraciəti",
                $"{request.OwnerName} tərəfindən \"{request.CarTitle}\" üçün yeni satış müraciəti göndərildi.",
                NotificationType.NewSellRequest,
                $"/Admin/SellCarRequest/Details/{request.Id}");

            return request;
        }

        // ?? ADMIN ?????????????????????????????????????????????????????????????

        public async Task<IList<SellCarRequest>> GetAllAdminAsync()
        {
            return await _context.SellCarRequests
                .AsNoTracking()
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
        }

        public async Task<IList<SellCarRequest>> GetPendingAsync()
        {
            return await _context.SellCarRequests
                .AsNoTracking()
                .Where(r => !r.IsReviewed)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
        }

        public async Task<int> GetPendingCountAsync()
        {
            return await _context.SellCarRequests
                .CountAsync(r => !r.IsReviewed);
        }

        public async Task<SellCarRequest?> GetByIdAsync(int id)
        {
            return await _context.SellCarRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task MarkAsReviewedAsync(int id)
        {
            var request = await _context.SellCarRequests.FindAsync(id)
                ?? throw new KeyNotFoundException($"Id={id} olan müraciət tapılmadı.");

            request.IsReviewed = true;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var request = await _context.SellCarRequests.FindAsync(id)
                ?? throw new KeyNotFoundException($"Id={id} olan müraciət tapılmadı.");

            _context.SellCarRequests.Remove(request);
            await _context.SaveChangesAsync();
        }
    }
}
