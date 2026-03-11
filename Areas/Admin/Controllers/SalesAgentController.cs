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
    public class SalesAgentController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IFileService _fileService;

        public SalesAgentController(ApplicationDbContext db, IFileService fileService)
        {
            _db = db;
            _fileService = fileService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["ActivePage"] = "SalesAgents";
            return View(await _db.SalesAgents.Include(a => a.Reviews).OrderBy(a => a.FullName).ToListAsync());
        }

        public IActionResult Create()
        {
            ViewData["ActivePage"] = "SalesAgents";
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SalesAgent agent, IFormFile? imageFile)
        {
            agent.CreatedDate = DateTime.UtcNow;
            if (imageFile != null)
                agent.ImageUrl = await _fileService.UploadAsync(imageFile, "uploads/agents");
            _db.SalesAgents.Add(agent);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Satış agenti əlavə edildi.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewData["ActivePage"] = "SalesAgents";
            var agent = await _db.SalesAgents.FindAsync(id);
            if (agent == null) return NotFound();
            return View(agent);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SalesAgent agent, IFormFile? imageFile)
        {
            var existing = await _db.SalesAgents.FindAsync(id);
            if (existing == null) return NotFound();
            existing.FullName = agent.FullName;
            existing.Title = agent.Title;
            existing.Bio = agent.Bio;
            existing.Address = agent.Address;
            existing.Phone1 = agent.Phone1;
            existing.Phone2 = agent.Phone2;
            existing.Email = agent.Email;
            existing.MapEmbedUrl = agent.MapEmbedUrl;
            existing.IsVerified = agent.IsVerified;
            existing.IsActive = agent.IsActive;
            existing.FacebookUrl = agent.FacebookUrl;
            existing.TwitterUrl = agent.TwitterUrl;
            existing.InstagramUrl = agent.InstagramUrl;
            existing.SkypeUrl = agent.SkypeUrl;
            existing.TelegramUrl = agent.TelegramUrl;
            if (imageFile != null)
                existing.ImageUrl = await _fileService.ReplaceAsync(existing.ImageUrl ?? "", imageFile, "uploads/agents");
            await _db.SaveChangesAsync();
            TempData["Success"] = "Satış agenti yeniləndi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var agent = await _db.SalesAgents.FindAsync(id);
            if (agent == null) return NotFound();
            if (!string.IsNullOrEmpty(agent.ImageUrl)) _fileService.Delete(agent.ImageUrl);
            _db.SalesAgents.Remove(agent);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Satış agenti silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
