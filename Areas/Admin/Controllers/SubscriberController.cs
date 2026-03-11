using Car_Project.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Car_Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class SubscriberController : Controller
    {
        private readonly ApplicationDbContext _db;
        public SubscriberController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            ViewData["ActivePage"] = "Subscribers";
            return View(await _db.NewsletterSubscribers.OrderByDescending(s => s.CreatedDate).ToListAsync());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var sub = await _db.NewsletterSubscribers.FindAsync(id);
            if (sub == null) return NotFound();
            _db.NewsletterSubscribers.Remove(sub);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Abunəçi silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
