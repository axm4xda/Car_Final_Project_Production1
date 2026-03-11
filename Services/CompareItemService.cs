using Car_Project.Data;
using Car_Project.Models;
using Car_Project.Services.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Car_Project.Services
{
    public class CompareItemService : ICompareItemService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private const string SessionKey = "CompareCarIds";
        private const int MaxCompareCount = 4;

        public CompareItemService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private ISession Session => _httpContextAccessor.HttpContext!.Session;

        private List<int> GetCarIds()
        {
            var json = Session.GetString(SessionKey);
            if (string.IsNullOrEmpty(json))
                return new List<int>();
            return JsonSerializer.Deserialize<List<int>>(json) ?? new List<int>();
        }

        private void SaveCarIds(List<int> carIds)
        {
            var json = JsonSerializer.Serialize(carIds);
            Session.SetString(SessionKey, json);
        }

        // ── PUBLIC ───────────────────────────────────────────────────────────

        public async Task<IList<Car>> GetCompareListAsync(string sessionId)
        {
            var carIds = GetCarIds();
            if (carIds.Count == 0)
                return new List<Car>();

            return await _context.Cars
                .AsNoTracking()
                .Where(c => carIds.Contains(c.Id))
                .Include(c => c.Brand)
                .Include(c => c.Images.Where(i => i.IsMain))
                .Include(c => c.Features)
                    .ThenInclude(f => f.CarFeature)
                .ToListAsync();
        }

        public Task<int> GetCountAsync(string sessionId)
        {
            return Task.FromResult(GetCarIds().Count);
        }

        public async Task AddAsync(string sessionId, int carId)
        {
            var carIds = GetCarIds();

            if (carIds.Count >= MaxCompareCount)
                throw new InvalidOperationException(
                    $"Compare list can hold a maximum of {MaxCompareCount} cars.");

            if (carIds.Contains(carId))
                throw new InvalidOperationException("This car is already in your compare list.");

            var carExists = await _context.Cars.AnyAsync(c => c.Id == carId);
            if (!carExists)
                throw new KeyNotFoundException($"Car with Id={carId} not found.");

            carIds.Add(carId);
            SaveCarIds(carIds);
        }

        public Task RemoveAsync(string sessionId, int carId)
        {
            var carIds = GetCarIds();
            carIds.Remove(carId);
            SaveCarIds(carIds);
            return Task.CompletedTask;
        }

        public Task ClearAsync(string sessionId)
        {
            Session.Remove(SessionKey);
            return Task.CompletedTask;
        }

        public Task<bool> IsInListAsync(string sessionId, int carId)
        {
            return Task.FromResult(GetCarIds().Contains(carId));
        }

        // ── ADMIN ────────────────────────────────────────────────────────────

        public Task CleanupExpiredAsync(DateTime olderThan)
        {
            // No-op for session-based storage; sessions expire automatically.
            return Task.CompletedTask;
        }
    }
}
