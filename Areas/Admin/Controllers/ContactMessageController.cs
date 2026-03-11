using Car_Project.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Car_Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class ContactMessageController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ContactMessageController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            ViewData["ActivePage"] = "ContactMessages";
            return View(await _db.ContactMessages.OrderByDescending(c => c.CreatedDate).ToListAsync());
        }

        public async Task<IActionResult> Details(int id)
        {
            ViewData["ActivePage"] = "ContactMessages";
            var msg = await _db.ContactMessages.FindAsync(id);
            if (msg == null) return NotFound();
            if (!msg.IsRead)
            {
                msg.IsRead = true;
                await _db.SaveChangesAsync();
            }
            return View(msg);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var msg = await _db.ContactMessages.FindAsync(id);
            if (msg == null) return NotFound();
            _db.ContactMessages.Remove(msg);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Mesaj silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
