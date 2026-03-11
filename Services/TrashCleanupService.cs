using Car_Project.Data;
using Car_Project.Models;
using Microsoft.EntityFrameworkCore;

namespace Car_Project.Services
{
    /// <summary>
    /// Zibil qutusundakı müraciətləri 10 gün sonra avtomatik silən background service.
    /// Hər 6 saatdan bir yoxlayır.
    /// </summary>
    public class TrashCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<TrashCleanupService> _logger;
        private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(6);
        private static readonly TimeSpan TrashRetention = TimeSpan.FromDays(10);

        public TrashCleanupService(IServiceScopeFactory scopeFactory, ILogger<TrashCleanupService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupTrashedRequestsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Zibil qutusu təmizləmə xətası.");
                }

                try
                {
                    await Task.Delay(CheckInterval, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // App bağlanır, normal çıxış
                    break;
                }
            }
        }

        private async Task CleanupTrashedRequestsAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var cutoff = DateTime.UtcNow - TrashRetention;

            var expiredItems = await db.SellCarRequests
                .Where(s => s.Status == SellCarRequestStatus.Trashed && s.TrashedDate != null && s.TrashedDate < cutoff)
                .ToListAsync();

            if (expiredItems.Count > 0)
            {
                db.SellCarRequests.RemoveRange(expiredItems);
                await db.SaveChangesAsync();
                _logger.LogInformation("Zibil qutusundan {Count} müddəti bitmiş müraciət silindi.", expiredItems.Count);
            }
        }
    }
}
