using Car_Project.Data;
using Car_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Car_Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class FaqController : Controller
    {
        private readonly ApplicationDbContext _db;
        public FaqController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            ViewData["ActivePage"] = "FAQs";
            return View(await _db.FAQs.OrderBy(f => f.Order).ThenBy(f => f.GroupName).ToListAsync());
        }

        public IActionResult Create()
        {
            ViewData["ActivePage"] = "FAQs";
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FAQ faq)
        {
            faq.CreatedDate = DateTime.UtcNow;
            _db.FAQs.Add(faq);
            await _db.SaveChangesAsync();
            TempData["Success"] = "FAQ əlavə edildi.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewData["ActivePage"] = "FAQs";
            var faq = await _db.FAQs.FindAsync(id);
            if (faq == null) return NotFound();
            return View(faq);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FAQ faq)
        {
            var existing = await _db.FAQs.FindAsync(id);
            if (existing == null) return NotFound();
            existing.Question = faq.Question;
            existing.Answer = faq.Answer;
            existing.GroupName = faq.GroupName;
            existing.Order = faq.Order;
            existing.IsActive = faq.IsActive;
            await _db.SaveChangesAsync();
            TempData["Success"] = "FAQ yeniləndi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var faq = await _db.FAQs.FindAsync(id);
            if (faq == null) return NotFound();
            _db.FAQs.Remove(faq);
            await _db.SaveChangesAsync();
            TempData["Success"] = "FAQ silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
