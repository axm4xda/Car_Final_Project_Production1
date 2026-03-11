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
    public class BrandController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IFileService _fileService;

        public BrandController(ApplicationDbContext db, IFileService fileService)
        {
            _db = db;
            _fileService = fileService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["ActivePage"] = "Brands";
            return View(await _db.Brands.Include(b => b.Cars).OrderBy(b => b.Name).ToListAsync());
        }

        public IActionResult Create()
        {
            ViewData["ActivePage"] = "Brands";
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Brand brand, IFormFile? logoFile)
        {
            brand.CreatedDate = DateTime.UtcNow;
            if (logoFile != null) brand.LogoUrl = await _fileService.UploadAsync(logoFile, "uploads/brands");
            _db.Brands.Add(brand);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Marka ?lav? edildi.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewData["ActivePage"] = "Brands";
            var brand = await _db.Brands.FindAsync(id);
            if (brand == null) return NotFound();
            return View(brand);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Brand brand, IFormFile? logoFile)
        {
            var existing = await _db.Brands.FindAsync(id);
            if (existing == null) return NotFound();
            existing.Name = brand.Name;
            if (logoFile != null) existing.LogoUrl = await _fileService.ReplaceAsync(existing.LogoUrl ?? "", logoFile, "uploads/brands");
            await _db.SaveChangesAsync();
            TempData["Success"] = "Marka yenil?ndi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var brand = await _db.Brands.FindAsync(id);
            if (brand == null) return NotFound();
            if (await _db.Cars.AnyAsync(c => c.BrandId == id))
            {
                TempData["Error"] = "Bu markaya aid ma??nlar var, ?vv?lc? onlar? silin.";
                return RedirectToAction(nameof(Index));
            }
            if (!string.IsNullOrEmpty(brand.LogoUrl)) _fileService.Delete(brand.LogoUrl);
            _db.Brands.Remove(brand);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Marka silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
