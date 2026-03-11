using Car_Project.Areas.Admin.ViewModels;
using Car_Project.Data;
using Car_Project.Models;
using Car_Project.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Car_Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class SellCarRequestController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IFileService _fileService;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;
        private readonly ILogger<SellCarRequestController> _logger;

        public SellCarRequestController(
            ApplicationDbContext db,
            IFileService fileService,
            INotificationService notificationService,
            IEmailService emailService,
            ILogger<SellCarRequestController> logger)
        {
            _db = db;
            _fileService = fileService;
            _notificationService = notificationService;
            _emailService = emailService;
            _logger = logger;
        }

        // GET: Satış Müraciətləri (Pending + Approved + Rejected)
        public async Task<IActionResult> Index(string? tab = null)
        {
            ViewData["ActivePage"] = "SellCarRequests";

            var requests = await _db.SellCarRequests
                .Where(s => s.Status != SellCarRequestStatus.Trashed)
                .OrderByDescending(s => s.CreatedDate)
                .ToListAsync();

            ViewBag.CurrentTab = tab ?? "all";
            return View(requests);
        }

        // GET: Zibil Qutusu — həm SellCarRequest, həm də Car trash-ı göstərir
        public async Task<IActionResult> Trash()
        {
            ViewData["ActivePage"] = "SellCarTrash";

            var trashedRequests = await _db.SellCarRequests
                .Where(s => s.Status == SellCarRequestStatus.Trashed)
                .OrderByDescending(s => s.TrashedDate)
                .ToListAsync();

            var trashedCars = await _db.Cars
                .Where(c => c.IsDeleted)
                .Include(c => c.Brand)
                .OrderByDescending(c => c.DeletedDate)
                .ToListAsync();

            var vm = new TrashViewModel
            {
                TrashedRequests = trashedRequests,
                TrashedCars = trashedCars
            };

            return View(vm);
        }

        // GET: Detal
        public async Task<IActionResult> Details(int id)
        {
            ViewData["ActivePage"] = "SellCarRequests";
            var req = await _db.SellCarRequests.FindAsync(id);
            if (req == null) return NotFound();

            // İlk dəfə baxılanda "oxunmuş" kimi işarələ
            if (!req.IsReviewed)
            {
                req.IsReviewed = true;
                await _db.SaveChangesAsync();
            }

            ViewBag.Brands = new SelectList(
                await _db.Brands.OrderBy(b => b.Name).ToListAsync(), "Id", "Name");

            return View(req);
        }

        // POST: Təsdiq et — Car yaradılır, List səhifəsinə düşür
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, int brandId, string? adminNote)
        {
            var req = await _db.SellCarRequests.FindAsync(id);
            if (req == null) return NotFound();

            FuelType fuelType = FuelType.Petrol;
            if (!string.IsNullOrEmpty(req.FuelType))
                Enum.TryParse(req.FuelType, true, out fuelType);

            TransmissionType transmission = TransmissionType.Automatic;
            if (!string.IsNullOrEmpty(req.Transmission))
                Enum.TryParse(req.Transmission, true, out transmission);

            var car = new Car
            {
                Title = req.CarTitle,
                Price = req.AskingPrice,
                Year = req.Year,
                Mileage = req.Mileage,
                FuelType = fuelType,
                Transmission = transmission,
                Condition = CarCondition.Used,
                Description = req.Description,
                ThumbnailUrl = req.ImageUrl,
                BrandId = brandId,
                Badge = "New Listing",
                BadgeColor = "bg-primary-2",
                CreatedDate = DateTime.UtcNow,
                IsApproved = true
            };

            _db.Cars.Add(car);
            await _db.SaveChangesAsync();

            if (!string.IsNullOrEmpty(req.ImageUrl))
            {
                _db.CarImages.Add(new CarImage
                {
                    CarId = car.Id,
                    ImageUrl = req.ImageUrl,
                    IsMain = true,
                    Order = 0,
                    CreatedDate = DateTime.UtcNow
                });
            }

            var brand = await _db.Brands.FindAsync(brandId);
            if (brand != null)
                brand.VehicleCount = await _db.Cars.CountAsync(c => c.BrandId == brandId && c.IsApproved);

            req.Status = SellCarRequestStatus.Approved;
            req.IsReviewed = true;
            req.AdminNote = adminNote;
            req.ApprovedCarId = car.Id;

            await _db.SaveChangesAsync();

            await _notificationService.CreateForAdminsAsync(
                "Satış Müraciəti Təsdiqləndi",
                $"\"{req.CarTitle}\" satış müraciəti təsdiqləndi və List səhifəsinə əlavə edildi.",
                NotificationType.SellRequestApproved,
                $"/Admin/Car/Edit/{car.Id}");

            // ── Müraciət sahibinə mail göndər (Approve) ──────────────────────
            if (!string.IsNullOrWhiteSpace(req.Email))
            {
                try
                {
                    await _emailService.SendSellRequestStatusAsync(
                        req.Email, req.OwnerName, req.CarTitle, true, adminNote);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Sell request approval email failed for {Email}", req.Email);
                }
            }

            TempData["Success"] = $"Müraciət təsdiqləndi! \"{car.Title}\" adlı maşın List səhifəsinə əlavə edildi.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Rədd et
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string? adminNote)
        {
            var req = await _db.SellCarRequests.FindAsync(id);
            if (req == null) return NotFound();

            req.Status = SellCarRequestStatus.Rejected;
            req.IsReviewed = true;
            req.AdminNote = adminNote;

            await _db.SaveChangesAsync();

            await _notificationService.CreateForAdminsAsync(
                "Satış Müraciəti Rədd Edildi",
                $"\"{req.CarTitle}\" satış müraciəti rədd edildi.",
                NotificationType.SellRequestRejected,
                $"/Admin/SellCarRequest/Details/{req.Id}");

            // ── Müraciət sahibinə mail göndər (Reject) ───────────────────────
            if (!string.IsNullOrWhiteSpace(req.Email))
            {
                try
                {
                    await _emailService.SendSellRequestStatusAsync(
                        req.Email, req.OwnerName, req.CarTitle, false, adminNote);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Sell request rejection email failed for {Email}", req.Email);
                }
            }

            TempData["Success"] = "Müraciət rədd edildi.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Zibil qutusuna at (soft delete)
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveToTrash(int id)
        {
            var req = await _db.SellCarRequests.FindAsync(id);
            if (req == null) return NotFound();

            req.Status = SellCarRequestStatus.Trashed;
            req.TrashedDate = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            TempData["Success"] = "Müraciət zibil qutusuna göndərildi. 10 gün sonra avtomatik silinəcək.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Zibil qutusundan bərpa et
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var req = await _db.SellCarRequests.FindAsync(id);
            if (req == null) return NotFound();

            req.Status = SellCarRequestStatus.Pending;
            req.TrashedDate = null;

            await _db.SaveChangesAsync();

            TempData["Success"] = "Müraciət bərpa edildi.";
            return RedirectToAction(nameof(Trash));
        }

        // POST: Həmişəlik sil (permanent delete)
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> PermanentDelete(int id)
        {
            var req = await _db.SellCarRequests.FindAsync(id);
            if (req == null) return NotFound();

            // Şəkil varsa sil
            if (!string.IsNullOrEmpty(req.ImageUrl))
                _fileService.Delete(req.ImageUrl);

            _db.SellCarRequests.Remove(req);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Müraciət həmişəlik silindi.";
            return RedirectToAction(nameof(Trash));
        }

        // POST: Zibil qutusunu boşalt (həm müraciətlər, həm maşınlar)
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EmptyTrash()
        {
            var trashedRequests = await _db.SellCarRequests
                .Where(s => s.Status == SellCarRequestStatus.Trashed)
                .ToListAsync();

            foreach (var req in trashedRequests)
            {
                if (!string.IsNullOrEmpty(req.ImageUrl))
                    _fileService.Delete(req.ImageUrl);
            }
            _db.SellCarRequests.RemoveRange(trashedRequests);

            var trashedCars = await _db.Cars
                .Where(c => c.IsDeleted)
                .Include(c => c.Images)
                .ToListAsync();

            foreach (var car in trashedCars)
            {
                if (!string.IsNullOrEmpty(car.ThumbnailUrl)) _fileService.Delete(car.ThumbnailUrl);
                foreach (var img in car.Images) _fileService.Delete(img.ImageUrl);
            }
            _db.Cars.RemoveRange(trashedCars);

            await _db.SaveChangesAsync();

            TempData["Success"] = $"{trashedRequests.Count} müraciət, {trashedCars.Count} maşın həmişəlik silindi.";
            return RedirectToAction(nameof(Trash));
        }
    }
}
