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
    public class ServiceCenterController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IFileService _fileService;

        public ServiceCenterController(ApplicationDbContext db, IFileService fileService)
        {
            _db = db;
            _fileService = fileService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["ActivePage"] = "ServiceCenters";
            return View(await _db.ServiceCenters.OrderBy(s => s.Name).ToListAsync());
        }

        public async Task<IActionResult> Details(int id)
        {
            ViewData["ActivePage"] = "ServiceCenters";
            var center = await _db.ServiceCenters.FindAsync(id);
            if (center == null) return NotFound();
            return View(center);
        }

        public IActionResult Create()
        {
            ViewData["ActivePage"] = "ServiceCenters";
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceCenter center, IFormFile? imageFile)
        {
            center.CreatedDate = DateTime.UtcNow;
            if (imageFile != null)
                center.ImageUrl = await _fileService.UploadAsync(imageFile, "uploads/service-centers");
            _db.ServiceCenters.Add(center);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Servis mərkəzi əlavə edildi.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewData["ActivePage"] = "ServiceCenters";
            var center = await _db.ServiceCenters.FindAsync(id);
            if (center == null) return NotFound();
            return View(center);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ServiceCenter center, IFormFile? imageFile)
        {
            var existing = await _db.ServiceCenters.FindAsync(id);
            if (existing == null) return NotFound();
            existing.Name = center.Name;
            existing.Address = center.Address;
            existing.Phone = center.Phone;
            existing.Email = center.Email;
            existing.WorkingHours = center.WorkingHours;
            existing.Latitude = center.Latitude;
            existing.Longitude = center.Longitude;
            if (imageFile != null)
                existing.ImageUrl = await _fileService.ReplaceAsync(existing.ImageUrl ?? "", imageFile, "uploads/service-centers");
            await _db.SaveChangesAsync();
            TempData["Success"] = "Servis mərkəzi yeniləndi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var center = await _db.ServiceCenters.FindAsync(id);
            if (center == null) return NotFound();
            if (!string.IsNullOrEmpty(center.ImageUrl)) _fileService.Delete(center.ImageUrl);
            _db.ServiceCenters.Remove(center);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Servis mərkəzi silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
