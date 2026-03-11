using Car_Project.Data;
using Car_Project.Models;
using Car_Project.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Car_Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class ReviewController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IFileService _fileService;

        public ReviewController(ApplicationDbContext db, IFileService fileService)
        {
            _db = db;
            _fileService = fileService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["ActivePage"] = "Reviews";
            return View(await _db.Reviews.OrderByDescending(r => r.CreatedDate).ToListAsync());
        }

        public IActionResult Create()
        {
            ViewData["ActivePage"] = "Reviews";
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Review review, IFormFile? avatarFile)
        {
            review.CreatedDate = DateTime.UtcNow;
            if (avatarFile != null)
                review.AvatarUrl = await _fileService.UploadAsync(avatarFile, "uploads/reviews");
            _db.Reviews.Add(review);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Rəy əlavə edildi.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewData["ActivePage"] = "Reviews";
            var review = await _db.Reviews.FindAsync(id);
            if (review == null) return NotFound();
            return View(review);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Review review, IFormFile? avatarFile)
        {
            var existing = await _db.Reviews.FindAsync(id);
            if (existing == null) return NotFound();
            existing.AuthorName = review.AuthorName;
            existing.AuthorTitle = review.AuthorTitle;
            existing.Content = review.Content;
            existing.Rating = review.Rating;
            existing.IsApproved = review.IsApproved;
            if (avatarFile != null)
                existing.AvatarUrl = await _fileService.ReplaceAsync(existing.AvatarUrl ?? "", avatarFile, "uploads/reviews");
            await _db.SaveChangesAsync();
            TempData["Success"] = "Rəy yeniləndi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleApprove(int id)
        {
            var review = await _db.Reviews.FindAsync(id);
            if (review == null) return NotFound();
            review.IsApproved = !review.IsApproved;
            await _db.SaveChangesAsync();
            TempData["Success"] = review.IsApproved ? "Rəy təsdiqləndi." : "Rəy geri alındı.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _db.Reviews.FindAsync(id);
            if (review == null) return NotFound();
            if (!string.IsNullOrEmpty(review.AvatarUrl)) _fileService.Delete(review.AvatarUrl);
            _db.Reviews.Remove(review);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Rəy silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
