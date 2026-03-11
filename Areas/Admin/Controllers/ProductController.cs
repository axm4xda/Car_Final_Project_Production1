using Car_Project.Data;
using Car_Project.Models;
using Car_Project.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Car_Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IFileService _fileService;

        public ProductController(ApplicationDbContext db, IFileService fileService)
        {
            _db = db;
            _fileService = fileService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["ActivePage"] = "Products";
            var products = await _db.Products
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
            return View(products);
        }

        public async Task<IActionResult> Create()
        {
            ViewData["ActivePage"] = "Products";
            ViewBag.Categories = new SelectList(await _db.ProductCategories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? thumbnailFile, List<IFormFile>? imageFiles)
        {
            product.CreatedDate = DateTime.UtcNow;
            if (string.IsNullOrEmpty(product.Slug))
                product.Slug = product.Name.ToLower().Replace(" ", "-");
            if (thumbnailFile != null)
                product.ThumbnailUrl = await _fileService.UploadAsync(thumbnailFile, "uploads/products");
            _db.Products.Add(product);
            await _db.SaveChangesAsync();
            if (imageFiles != null)
            {
                bool first = true;
                int order = 0;
                foreach (var f in imageFiles)
                {
                    var url = await _fileService.UploadAsync(f, "uploads/products");
                    _db.ProductImages.Add(new ProductImage { ProductId = product.Id, ImageUrl = url, IsMain = first, Order = order++, CreatedDate = DateTime.UtcNow });
                    first = false;
                }
                await _db.SaveChangesAsync();
            }
            TempData["Success"] = "Məhsul əlavə edildi.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewData["ActivePage"] = "Products";
            var product = await _db.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();
            ViewBag.Categories = new SelectList(await _db.ProductCategories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", product.CategoryId);
            return View(product);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile? thumbnailFile, List<IFormFile>? imageFiles)
        {
            var existing = await _db.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
            if (existing == null) return NotFound();
            existing.Name = product.Name;
            existing.Slug = string.IsNullOrEmpty(product.Slug) ? product.Name.ToLower().Replace(" ", "-") : product.Slug;
            existing.Description = product.Description;
            existing.Price = product.Price;
            existing.OldPrice = product.OldPrice;
            existing.Stock = product.Stock;
            existing.Badge = product.Badge;
            existing.BadgeColor = product.BadgeColor;
            existing.IsActive = product.IsActive;
            existing.IsFeatured = product.IsFeatured;
            existing.CategoryId = product.CategoryId;
            if (thumbnailFile != null)
                existing.ThumbnailUrl = await _fileService.ReplaceAsync(existing.ThumbnailUrl ?? "", thumbnailFile, "uploads/products");
            if (imageFiles is { Count: > 0 })
            {
                var maxOrder = existing.Images.Any() ? existing.Images.Max(i => i.Order) : 0;
                foreach (var f in imageFiles)
                {
                    var url = await _fileService.UploadAsync(f, "uploads/products");
                    _db.ProductImages.Add(new ProductImage { ProductId = id, ImageUrl = url, Order = ++maxOrder, CreatedDate = DateTime.UtcNow });
                }
            }
            await _db.SaveChangesAsync();
            TempData["Success"] = "Məhsul yeniləndi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _db.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();
            if (!string.IsNullOrEmpty(product.ThumbnailUrl)) _fileService.Delete(product.ThumbnailUrl);
            foreach (var img in product.Images) _fileService.Delete(img.ImageUrl);
            _db.Products.Remove(product);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Məhsul silindi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(int id)
        {
            var img = await _db.ProductImages.FindAsync(id);
            if (img == null) return NotFound();
            _fileService.Delete(img.ImageUrl);
            _db.ProductImages.Remove(img);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Şəkil silindi.";
            return RedirectToAction(nameof(Edit), new { id = img.ProductId });
        }
    }
}
