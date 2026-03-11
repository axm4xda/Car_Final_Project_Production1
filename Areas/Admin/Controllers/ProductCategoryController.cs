using Car_Project.Data;
using Car_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Car_Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class ProductCategoryController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ProductCategoryController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            ViewData["ActivePage"] = "ProductCategories";
            return View(await _db.ProductCategories.Include(c => c.Products).OrderBy(c => c.Name).ToListAsync());
        }

        public IActionResult Create()
        {
            ViewData["ActivePage"] = "ProductCategories";
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCategory category)
        {
            category.CreatedDate = DateTime.UtcNow;
            if (string.IsNullOrEmpty(category.Slug))
                category.Slug = category.Name.ToLower().Replace(" ", "-");
            _db.ProductCategories.Add(category);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Məhsul kateqoriyası əlavə edildi.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewData["ActivePage"] = "ProductCategories";
            var cat = await _db.ProductCategories.FindAsync(id);
            if (cat == null) return NotFound();
            return View(cat);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductCategory category)
        {
            var existing = await _db.ProductCategories.FindAsync(id);
            if (existing == null) return NotFound();
            existing.Name = category.Name;
            existing.Slug = string.IsNullOrEmpty(category.Slug) ? category.Name.ToLower().Replace(" ", "-") : category.Slug;
            existing.IconUrl = category.IconUrl;
            await _db.SaveChangesAsync();
            TempData["Success"] = "Kateqoriya yeniləndi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var cat = await _db.ProductCategories.Include(c => c.Products).FirstOrDefaultAsync(c => c.Id == id);
            if (cat == null) return NotFound();
            if (cat.Products.Any())
            {
                TempData["Error"] = "Bu kateqoriyaya aid məhsullar var, əvvəlcə onları silin.";
                return RedirectToAction(nameof(Index));
            }
            _db.ProductCategories.Remove(cat);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Kateqoriya silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
