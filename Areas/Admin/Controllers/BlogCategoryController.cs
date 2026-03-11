using Car_Project.Data;
using Car_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Car_Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class BlogCategoryController : Controller
    {
        private readonly ApplicationDbContext _db;
        public BlogCategoryController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            ViewData["ActivePage"] = "BlogCategories";
            return View(await _db.BlogCategories.Include(c => c.Posts).OrderBy(c => c.Name).ToListAsync());
        }

        public IActionResult Create()
        {
            ViewData["ActivePage"] = "BlogCategories";
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BlogCategory category)
        {
            category.CreatedDate = DateTime.UtcNow;
            if (string.IsNullOrEmpty(category.Slug))
                category.Slug = category.Name.ToLower().Replace(" ", "-");
            _db.BlogCategories.Add(category);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Blog kateqoriyas? ?lav? edildi.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewData["ActivePage"] = "BlogCategories";
            var cat = await _db.BlogCategories.FindAsync(id);
            if (cat == null) return NotFound();
            return View(cat);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BlogCategory category)
        {
            var existing = await _db.BlogCategories.FindAsync(id);
            if (existing == null) return NotFound();
            existing.Name = category.Name;
            existing.Slug = string.IsNullOrEmpty(category.Slug) ? category.Name.ToLower().Replace(" ", "-") : category.Slug;
            await _db.SaveChangesAsync();
            TempData["Success"] = "Kateqoriya yenil?ndi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var cat = await _db.BlogCategories.Include(c => c.Posts).FirstOrDefaultAsync(c => c.Id == id);
            if (cat == null) return NotFound();
            if (cat.Posts.Any())
            {
                TempData["Error"] = "Bu kateqoriyaya aid yaz?lar var, ?vv?lc? onlar? silin.";
                return RedirectToAction(nameof(Index));
            }
            _db.BlogCategories.Remove(cat);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Kateqoriya silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
