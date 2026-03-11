using Car_Project.Data;
using Car_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Car_Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class CouponController : Controller
    {
        private readonly ApplicationDbContext _db;
        public CouponController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            ViewData["ActivePage"] = "Coupons";
            return View(await _db.Coupons.OrderByDescending(c => c.CreatedDate).ToListAsync());
        }

        public IActionResult Create()
        {
            ViewData["ActivePage"] = "Coupons";
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Coupon coupon)
        {
            coupon.CreatedDate = DateTime.UtcNow;
            coupon.Code = coupon.Code.ToUpper().Trim();
            _db.Coupons.Add(coupon);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Kupon əlavə edildi.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewData["ActivePage"] = "Coupons";
            var coupon = await _db.Coupons.FindAsync(id);
            if (coupon == null) return NotFound();
            return View(coupon);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Coupon coupon)
        {
            var existing = await _db.Coupons.FindAsync(id);
            if (existing == null) return NotFound();
            existing.Code = coupon.Code.ToUpper().Trim();
            existing.DiscountPercent = coupon.DiscountPercent;
            existing.DiscountAmount = coupon.DiscountAmount;
            existing.MinOrderAmount = coupon.MinOrderAmount;
            existing.IsActive = coupon.IsActive;
            existing.ExpiresAt = coupon.ExpiresAt;
            existing.UsageLimit = coupon.UsageLimit;
            await _db.SaveChangesAsync();
            TempData["Success"] = "Kupon yeniləndi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var coupon = await _db.Coupons.FindAsync(id);
            if (coupon == null) return NotFound();
            _db.Coupons.Remove(coupon);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Kupon silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
