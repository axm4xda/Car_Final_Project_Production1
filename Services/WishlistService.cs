using Car_Project.Data;
using Car_Project.Models;
using Car_Project.Services.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Car_Project.Services
{
    public class WishlistService : IWishlistService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private const string SessionKey = "WishlistCarIds";

        public WishlistService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
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

        public async Task<IList<Car>> GetWishlistAsync(string sessionId)
        {
            var carIds = GetCarIds();
            if (carIds.Count == 0)
                return new List<Car>();

            return await _context.Cars
                .AsNoTracking()
                .Where(c => carIds.Contains(c.Id))
                .Include(c => c.Brand)
                .Include(c => c.Images.Where(i => i.IsMain))
                .ToListAsync();
        }

        public Task<int> GetCountAsync(string sessionId)
        {
            return Task.FromResult(GetCarIds().Count);
        }

        public async Task AddAsync(string sessionId, int carId)
        {
            var carIds = GetCarIds();
            if (carIds.Contains(carId))
                return; // Already in wishlist — no duplicate

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

        public Task<bool> IsInWishlistAsync(string sessionId, int carId)
        {
            return Task.FromResult(GetCarIds().Contains(carId));
        }
    }
}
