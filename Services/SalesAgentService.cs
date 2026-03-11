using Car_Project.Data;
using Car_Project.Models;
using Car_Project.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Car_Project.Services
{
    public class SalesAgentService : ISalesAgentService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public SalesAgentService(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<IList<SalesAgent>> GetAllActiveAsync()
        {
            return await _context.SalesAgents
                .AsNoTracking()
                .Where(a => a.IsActive)
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();
        }

        public async Task<SalesAgent?> GetByIdAsync(int id)
        {
            return await _context.SalesAgents
                .AsNoTracking()
                .Include(a => a.Reviews.Where(r => r.IsApproved))
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<SalesAgent> CreateAsync(SalesAgent agent)
        {
            if (agent == null) throw new ArgumentNullException(nameof(agent));
            agent.CreatedDate = DateTime.UtcNow;
            await _context.SalesAgents.AddAsync(agent);
            await _context.SaveChangesAsync();
            return agent;
        }

        public async Task UpdateAsync(SalesAgent agent)
        {
            if (agent == null) throw new ArgumentNullException(nameof(agent));

            var existing = await _context.SalesAgents.FindAsync(agent.Id)
                ?? throw new KeyNotFoundException($"Id={agent.Id} olan agent tapılmadı.");

            existing.FullName     = agent.FullName;
            existing.Title        = agent.Title;
            existing.ImageUrl     = agent.ImageUrl;
            existing.Bio          = agent.Bio;
            existing.Address      = agent.Address;
            existing.Phone1       = agent.Phone1;
            existing.Phone2       = agent.Phone2;
            existing.Email        = agent.Email;
            existing.MapEmbedUrl  = agent.MapEmbedUrl;
            existing.IsVerified   = agent.IsVerified;
            existing.IsActive     = agent.IsActive;
            existing.FacebookUrl  = agent.FacebookUrl;
            existing.TwitterUrl   = agent.TwitterUrl;
            existing.InstagramUrl = agent.InstagramUrl;
            existing.SkypeUrl     = agent.SkypeUrl;
            existing.TelegramUrl  = agent.TelegramUrl;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var agent = await _context.SalesAgents.FindAsync(id)
                ?? throw new KeyNotFoundException($"Id={id} olan agent tapılmadı.");

            _context.SalesAgents.Remove(agent);
            await _context.SaveChangesAsync();
        }

        public async Task AddReviewAsync(SalesAgentReview review)
        {
            if (review == null) throw new ArgumentNullException(nameof(review));
            review.CreatedDate = DateTime.UtcNow;
            review.IsApproved  = true; // admin təsdiqsiz birbaşa göstər (istəyə görə false edə bilərsiniz)
            await _context.SalesAgentReviews.AddAsync(review);
            await _context.SaveChangesAsync();

            // Agent adını əldə et
            var agent = await _context.SalesAgents.AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == review.SalesAgentId);
            var agentName = agent?.FullName ?? "Agent";

            // Adminlərə yeni agent rəyi bildirişi göndər
            await _notificationService.CreateForAdminsAsync(
                "Yeni Agent Rəyi",
                $"{review.AuthorName} tərəfindən {agentName} agentinə {review.Rating}⭐ rəy əlavə edildi.",
                NotificationType.NewReview,
                $"/Admin/SalesAgent/Index");
        }
    }
}
