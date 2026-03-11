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
    public class BlogPostController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IFileService _fileService;

        public BlogPostController(ApplicationDbContext db, IFileService fileService)
        {
            _db = db;
            _fileService = fileService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["ActivePage"] = "BlogPosts";
            var posts = await _db.BlogPosts
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
            return View(posts);
        }

        public async Task<IActionResult> Create()
        {
            ViewData["ActivePage"] = "BlogPosts";
            ViewBag.Categories = new SelectList(await _db.BlogCategories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BlogPost post, IFormFile? thumbnailFile, IFormFile? authorAvatarFile)
        {
            post.CreatedDate = DateTime.UtcNow;
            if (string.IsNullOrEmpty(post.Slug))
                post.Slug = post.Title.ToLower().Replace(" ", "-");
            if (post.Status == BlogStatus.Published)
                post.PublishedAt = DateTime.UtcNow;
            post.AuthorFacebookUrl  = string.IsNullOrWhiteSpace(post.AuthorFacebookUrl)  ? null : post.AuthorFacebookUrl;
            post.AuthorTwitterUrl   = string.IsNullOrWhiteSpace(post.AuthorTwitterUrl)   ? null : post.AuthorTwitterUrl;
            post.AuthorInstagramUrl = string.IsNullOrWhiteSpace(post.AuthorInstagramUrl) ? null : post.AuthorInstagramUrl;
            post.AuthorLinkedInUrl  = string.IsNullOrWhiteSpace(post.AuthorLinkedInUrl)  ? null : post.AuthorLinkedInUrl;
            if (thumbnailFile != null)
                post.ThumbnailUrl = await _fileService.UploadAsync(thumbnailFile, "uploads/blog");
            if (authorAvatarFile != null)
                post.AuthorAvatarUrl = await _fileService.UploadAsync(authorAvatarFile, "uploads/blog/avatars");
            _db.BlogPosts.Add(post);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Blog yazısı əlavə edildi.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewData["ActivePage"] = "BlogPosts";
            var post = await _db.BlogPosts.FindAsync(id);
            if (post == null) return NotFound();
            ViewBag.Categories = new SelectList(await _db.BlogCategories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", post.CategoryId);
            return View(post);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BlogPost post, IFormFile? thumbnailFile, IFormFile? authorAvatarFile)
        {
            var existing = await _db.BlogPosts.FindAsync(id);
            if (existing == null) return NotFound();
            existing.Title      = post.Title;
            existing.Slug       = string.IsNullOrEmpty(post.Slug) ? post.Title.ToLower().Replace(" ", "-") : post.Slug;
            existing.Summary    = post.Summary;
            existing.Content    = post.Content;
            existing.AuthorName = post.AuthorName;
            existing.CategoryId = post.CategoryId;
            existing.Status     = post.Status;
            existing.AuthorFacebookUrl  = string.IsNullOrWhiteSpace(post.AuthorFacebookUrl)  ? null : post.AuthorFacebookUrl;
            existing.AuthorTwitterUrl   = string.IsNullOrWhiteSpace(post.AuthorTwitterUrl)   ? null : post.AuthorTwitterUrl;
            existing.AuthorInstagramUrl = string.IsNullOrWhiteSpace(post.AuthorInstagramUrl) ? null : post.AuthorInstagramUrl;
            existing.AuthorLinkedInUrl  = string.IsNullOrWhiteSpace(post.AuthorLinkedInUrl)  ? null : post.AuthorLinkedInUrl;
            if (post.Status == BlogStatus.Published && existing.PublishedAt == null)
                existing.PublishedAt = DateTime.UtcNow;
            if (thumbnailFile != null)
                existing.ThumbnailUrl = await _fileService.ReplaceAsync(existing.ThumbnailUrl ?? "", thumbnailFile, "uploads/blog");
            if (authorAvatarFile != null)
                existing.AuthorAvatarUrl = await _fileService.ReplaceAsync(existing.AuthorAvatarUrl ?? "", authorAvatarFile, "uploads/blog/avatars");
            await _db.SaveChangesAsync();
            TempData["Success"] = "Blog yazısı yeniləndi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _db.BlogPosts.FindAsync(id);
            if (post == null) return NotFound();
            if (!string.IsNullOrEmpty(post.ThumbnailUrl)) _fileService.Delete(post.ThumbnailUrl);
            _db.BlogPosts.Remove(post);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Blog yazısı silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
