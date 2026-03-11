using Car_Project.Data;
using Car_Project.Models;
using Car_Project.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Car_Project.Services
{
    public class CouponService : ICouponService
    {
        private readonly ApplicationDbContext _context;

        public CouponService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Coupon?> ValidateAsync(string code, decimal orderTotal)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;

            var coupon = await _context.Coupons
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Code == code && c.IsActive);

            if (coupon == null) return null;

            // Vaxt yoxlaması
            if (coupon.ExpiresAt.HasValue && coupon.ExpiresAt.Value < DateTime.UtcNow)
                return null;

            // İstifadə limiti
            if (coupon.UsageLimit > 0 && coupon.UsedCount >= coupon.UsageLimit)
                return null;

            // Minimum məbləğ
            if (orderTotal < coupon.MinOrderAmount)
                return null;

            return coupon;
        }

        public async Task UseAsync(string code)
        {
            var coupon = await _context.Coupons
                .FirstOrDefaultAsync(c => c.Code == code);
            if (coupon == null) return;

            coupon.UsedCount++;
            await _context.SaveChangesAsync();
        }

        public async Task<IList<Coupon>> GetAllAdminAsync() =>
            await _context.Coupons
                .AsNoTracking()
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();

        public async Task<Coupon?> GetByIdAsync(int id) =>
            await _context.Coupons
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

        public async Task<Coupon> CreateAsync(Coupon coupon)
        {
            if (coupon == null) throw new ArgumentNullException(nameof(coupon));

            var exists = await _context.Coupons.AnyAsync(c => c.Code == coupon.Code);
            if (exists)
                throw new InvalidOperationException($"'{coupon.Code}' kodu artıq mövcuddur.");

            coupon.CreatedDate = DateTime.UtcNow;
            await _context.Coupons.AddAsync(coupon);
            await _context.SaveChangesAsync();
            return coupon;
        }

        public async Task UpdateAsync(Coupon coupon)
        {
            var existing = await _context.Coupons.FindAsync(coupon.Id)
                ?? throw new KeyNotFoundException($"Id={coupon.Id} olan kupon tapılmadı.");

            existing.Code            = coupon.Code;
            existing.DiscountPercent = coupon.DiscountPercent;
            existing.DiscountAmount  = coupon.DiscountAmount;
            existing.MinOrderAmount  = coupon.MinOrderAmount;
            existing.IsActive        = coupon.IsActive;
            existing.ExpiresAt       = coupon.ExpiresAt;
            existing.UsageLimit      = coupon.UsageLimit;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var coupon = await _context.Coupons.FindAsync(id)
                ?? throw new KeyNotFoundException($"Id={id} olan kupon tapılmadı.");
            _context.Coupons.Remove(coupon);
            await _context.SaveChangesAsync();
        }

        public async Task ToggleActiveAsync(int id)
        {
            var coupon = await _context.Coupons.FindAsync(id)
                ?? throw new KeyNotFoundException($"Id={id} olan kupon tapılmadı.");
            coupon.IsActive = !coupon.IsActive;
            await _context.SaveChangesAsync();
        }

        public async Task<Coupon> GenerateForUserAsync(string userId)
        {
            // Generate unique coupon code
            string code;
            do
            {
                code = $"THANK-{Guid.NewGuid():N}"[..16].ToUpper();
            }
            while (await _context.Coupons.AnyAsync(c => c.Code == code));

            var coupon = new Coupon
            {
                Code            = code,
                DiscountPercent = 15,
                DiscountAmount  = null,
                MinOrderAmount  = 0,
                IsActive        = true,
                ExpiresAt       = DateTime.UtcNow.AddDays(30),
                UsageLimit      = 1,
                UsedCount       = 0,
                UserId          = userId,
                CreatedDate     = DateTime.UtcNow
            };

            await _context.Coupons.AddAsync(coupon);
            await _context.SaveChangesAsync();
            return coupon;
        }
    }
}
