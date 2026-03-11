using Car_Project.Data;
using Car_Project.Models;
using Car_Project.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Car_Project.Services
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;

        public CartService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IList<CartItem>> GetCartAsync(string sessionId)
        {
            ValidateSession(sessionId);
            return await _context.CartItems
                .AsNoTracking()
                .Include(ci => ci.Product)
                    .ThenInclude(p => p.Images.Where(i => i.IsMain))
                .Where(ci => ci.SessionId == sessionId)
                .ToListAsync();
        }

        public async Task<int> GetCartCountAsync(string sessionId)
        {
            ValidateSession(sessionId);
            return await _context.CartItems
                .Where(ci => ci.SessionId == sessionId)
                .SumAsync(ci => ci.Quantity);
        }

        public async Task AddToCartAsync(string sessionId, int productId, int quantity = 1)
        {
            ValidateSession(sessionId);
            if (quantity < 1) throw new ArgumentOutOfRangeException(nameof(quantity));

            var product = await _context.Products.FindAsync(productId)
                ?? throw new KeyNotFoundException($"Id={productId} olan məhsul tapılmadı.");

            if (product.Stock < quantity)
                throw new InvalidOperationException("Stokda yetərli məhsul yoxdur.");

            var existing = await _context.CartItems
                .FirstOrDefaultAsync(ci =>
                    ci.SessionId == sessionId && ci.ProductId == productId);

            if (existing != null)
                existing.Quantity += quantity;
            else
                await _context.CartItems.AddAsync(new CartItem
                {
                    SessionId   = sessionId,
                    ProductId   = productId,
                    Quantity    = quantity,
                    CreatedDate = DateTime.UtcNow
                });

            await _context.SaveChangesAsync();
        }

        public async Task UpdateQuantityAsync(string sessionId, int productId, int quantity)
        {
            ValidateSession(sessionId);
            if (quantity < 1) throw new ArgumentOutOfRangeException(nameof(quantity));

            var item = await _context.CartItems
                .FirstOrDefaultAsync(ci =>
                    ci.SessionId == sessionId && ci.ProductId == productId)
                ?? throw new KeyNotFoundException("Səbət elementi tapılmadı.");

            item.Quantity = quantity;
            await _context.SaveChangesAsync();
        }

        public async Task RemoveFromCartAsync(string sessionId, int productId)
        {
            ValidateSession(sessionId);
            var item = await _context.CartItems
                .FirstOrDefaultAsync(ci =>
                    ci.SessionId == sessionId && ci.ProductId == productId);
            if (item == null) return;

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();
        }

        public async Task ClearCartAsync(string sessionId)
        {
            ValidateSession(sessionId);
            var items = await _context.CartItems
                .Where(ci => ci.SessionId == sessionId)
                .ToListAsync();
            if (items.Count > 0)
            {
                _context.CartItems.RemoveRange(items);
                await _context.SaveChangesAsync();
            }
        }

        public async Task CleanupExpiredAsync(DateTime olderThan)
        {
            var expired = await _context.CartItems
                .Where(ci => ci.CreatedDate < olderThan)
                .ToListAsync();
            if (expired.Count > 0)
            {
                _context.CartItems.RemoveRange(expired);
                await _context.SaveChangesAsync();
            }
        }

        private static void ValidateSession(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                throw new ArgumentException("Session ID boş ola bilməz.", nameof(sessionId));
        }
    }
}
