using Car_Project.Data;
using Car_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Car_Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class CarFeatureController : Controller
    {
        private readonly ApplicationDbContext _db;
        public CarFeatureController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            ViewData["ActivePage"] = "CarFeatures";
            return View(await _db.CarFeatures.Include(f => f.Cars).OrderBy(f => f.Name).ToListAsync());
        }

        public IActionResult Create()
        {
            ViewData["ActivePage"] = "CarFeatures";
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CarFeature feature)
        {
            feature.CreatedDate = DateTime.UtcNow;
            _db.CarFeatures.Add(feature);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Xüsusiyy?t ?lav? edildi.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewData["ActivePage"] = "CarFeatures";
            var f = await _db.CarFeatures.FindAsync(id);
            if (f == null) return NotFound();
            return View(f);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CarFeature feature)
        {
            var existing = await _db.CarFeatures.FindAsync(id);
            if (existing == null) return NotFound();
            existing.Name = feature.Name;
            await _db.SaveChangesAsync();
            TempData["Success"] = "Xüsusiyy?t yenil?ndi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var f = await _db.CarFeatures.FindAsync(id);
            if (f == null) return NotFound();
            _db.CarFeatures.Remove(f);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Xüsusiyy?t silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
