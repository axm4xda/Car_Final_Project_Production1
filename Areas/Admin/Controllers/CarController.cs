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
    public class CarController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IFileService _fileService;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;
        private readonly ILogger<CarController> _logger;

        public CarController(
            ApplicationDbContext db,
            IFileService fileService,
            INotificationService notificationService,
            IEmailService emailService,
            ILogger<CarController> logger)
        {
            _db = db;
            _fileService = fileService;
            _notificationService = notificationService;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["ActivePage"] = "Cars";
            var cars = await _db.Cars
                .Where(c => !c.IsDeleted)
                .Include(c => c.Brand).Include(c => c.Images)
                .OrderByDescending(c => c.CreatedDate).ToListAsync();
            return View(cars);
        }

        public async Task<IActionResult> Create()
        {
            ViewData["ActivePage"] = "Cars";
            ViewBag.Brands = new SelectList(await _db.Brands.OrderBy(b => b.Name).ToListAsync(), "Id", "Name");
            ViewBag.AllFeatures = await _db.CarFeatures.OrderBy(f => f.Name).ToListAsync();
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Car car, IFormFile? thumbnailFile, List<IFormFile>? imageFiles, int[]? featureIds)
        {
            car.CreatedDate = DateTime.UtcNow;
            car.IsApproved = true; // Admin tərəfindən əlavə edilən maşınlar avtomatik təsdiqlənir

            if (thumbnailFile != null)
                car.ThumbnailUrl = await _fileService.UploadAsync(thumbnailFile, "uploads/cars");

            _db.Cars.Add(car);
            await _db.SaveChangesAsync();

            if (imageFiles != null)
            {
                int order = 0;
                foreach (var f in imageFiles)
                {
                    var url = await _fileService.UploadAsync(f, "uploads/cars");
                    _db.CarImages.Add(new CarImage { CarId = car.Id, ImageUrl = url, Order = order++, CreatedDate = DateTime.UtcNow });
                }
            }
            if (featureIds != null)
                foreach (var fid in featureIds)
                    _db.CarFeatureMappings.Add(new CarFeatureMapping { CarId = car.Id, CarFeatureId = fid });

            var brand = await _db.Brands.FindAsync(car.BrandId);
            if (brand != null)
                brand.VehicleCount = await _db.Cars.CountAsync(c => c.BrandId == car.BrandId && c.IsApproved);

            await _db.SaveChangesAsync();
            TempData["Success"] = "Maşın uğurla əlavə edildi.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewData["ActivePage"] = "Cars";
            var car = await _db.Cars.Include(c => c.Features).Include(c => c.Images).FirstOrDefaultAsync(c => c.Id == id);
            if (car == null) return NotFound();
            ViewBag.Brands = new SelectList(await _db.Brands.OrderBy(b => b.Name).ToListAsync(), "Id", "Name", car.BrandId);
            ViewBag.AllFeatures = await _db.CarFeatures.OrderBy(f => f.Name).ToListAsync();
            ViewBag.SelectedFeatureIds = car.Features.Select(f => f.CarFeatureId).ToList();
            return View(car);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Car car, IFormFile? thumbnailFile, List<IFormFile>? imageFiles, int[]? featureIds)
        {
            var existing = await _db.Cars.Include(c => c.Features).FirstOrDefaultAsync(c => c.Id == id);
            if (existing == null) return NotFound();

            existing.Title = car.Title; existing.Price = car.Price; existing.MonthlyPayment = car.MonthlyPayment;
            existing.Year = car.Year; existing.Mileage = car.Mileage; existing.FuelType = car.FuelType;
            existing.Transmission = car.Transmission; existing.Condition = car.Condition;
            existing.BodyStyle = car.BodyStyle; existing.DriveType = car.DriveType;
            existing.Color = car.Color; existing.InteriorColor = car.InteriorColor;
            existing.Cylinders = car.Cylinders; existing.DoorCount = car.DoorCount;
            existing.Description = car.Description; existing.BrandId = car.BrandId;
            existing.Badge = car.Badge; existing.BadgeColor = car.BadgeColor;

            if (thumbnailFile != null)
                existing.ThumbnailUrl = await _fileService.ReplaceAsync(existing.ThumbnailUrl ?? "", thumbnailFile, "uploads/cars");

            _db.CarFeatureMappings.RemoveRange(existing.Features);
            if (featureIds != null)
                foreach (var fid in featureIds)
                    _db.CarFeatureMappings.Add(new CarFeatureMapping { CarId = id, CarFeatureId = fid });

            if (imageFiles is { Count: > 0 })
            {
                var maxOrder = await _db.CarImages.Where(ci => ci.CarId == id).MaxAsync(ci => (int?)ci.Order) ?? 0;
                foreach (var f in imageFiles)
                {
                    var url = await _fileService.UploadAsync(f, "uploads/cars");
                    _db.CarImages.Add(new CarImage { CarId = id, ImageUrl = url, Order = ++maxOrder, CreatedDate = DateTime.UtcNow });
                }
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = "Maşın yeniləndi.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Elanı təsdiqlə
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var car = await _db.Cars.Include(c => c.User).FirstOrDefaultAsync(c => c.Id == id);
            if (car == null) return NotFound();

            car.IsApproved = true;
            await _db.SaveChangesAsync();

            var brand = await _db.Brands.FindAsync(car.BrandId);
            if (brand != null)
            {
                brand.VehicleCount = await _db.Cars.CountAsync(c => c.BrandId == car.BrandId && c.IsApproved && !c.IsDeleted);
                await _db.SaveChangesAsync();
            }

            if (!string.IsNullOrEmpty(car.UserId))
            {
                await _notificationService.CreateForUserAsync(
                    car.UserId,
                    "Elanınız Təsdiqləndi ✅",
                    $"\"{car.Title}\" adlı elanınız admin tərəfindən təsdiqləndi və artıq List səhifəsində yayımlanır.",
                    NotificationType.CarApproved,
                    $"/List/Detail/{car.Id}");

                // ── Mail bildirişi ────────────────────────────────────────────
                var ownerEmail = car.User?.Email;
                if (!string.IsNullOrWhiteSpace(ownerEmail))
                {
                    try
                    {
                        await _emailService.SendCarListingStatusAsync(
                            ownerEmail,
                            car.User?.FullName ?? "İstifadəçi",
                            car.Title,
                            true,
                            null);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Car approval email failed for {Email}", ownerEmail);
                    }
                }
            }

            TempData["Success"] = $"\"{car.Title}\" elanı təsdiqləndi və List səhifəsində yayımlandı.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Elanın təsdiqini ləğv et
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var car = await _db.Cars.Include(c => c.User).FirstOrDefaultAsync(c => c.Id == id);
            if (car == null) return NotFound();

            car.IsApproved = false;
            await _db.SaveChangesAsync();

            var brand = await _db.Brands.FindAsync(car.BrandId);
            if (brand != null)
            {
                brand.VehicleCount = await _db.Cars.CountAsync(c => c.BrandId == car.BrandId && c.IsApproved && !c.IsDeleted);
                await _db.SaveChangesAsync();
            }

            if (!string.IsNullOrEmpty(car.UserId))
            {
                await _notificationService.CreateForUserAsync(
                    car.UserId,
                    "Elanınız Rədd Edildi ❌",
                    $"\"{car.Title}\" adlı elanınız admin tərəfindən rədd edildi. Daha ətraflı məlumat üçün bizimlə əlaqə saxlayın.",
                    NotificationType.CarRejected,
                    $"/UserListing/Index");

                // ── Mail bildirişi ────────────────────────────────────────────
                var ownerEmail = car.User?.Email;
                if (!string.IsNullOrWhiteSpace(ownerEmail))
                {
                    try
                    {
                        await _emailService.SendCarListingStatusAsync(
                            ownerEmail,
                            car.User?.FullName ?? "İstifadəçi",
                            car.Title,
                            false,
                            null);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Car rejection email failed for {Email}", ownerEmail);
                    }
                }
            }

            TempData["Success"] = $"\"{car.Title}\" elanının təsdiqi ləğv edildi.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Maşını zibil qutusuna at (soft delete) — Admin/SellCarRequest/Trash-da görünür
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var car = await _db.Cars.Include(c => c.Images).FirstOrDefaultAsync(c => c.Id == id);
            if (car == null) return NotFound();

            car.IsDeleted = true;
            car.DeletedDate = DateTime.UtcNow;
            car.IsApproved = false;

            // Markanın maşın sayını yenilə
            var brand = await _db.Brands.FindAsync(car.BrandId);
            if (brand != null)
            {
                brand.VehicleCount = await _db.Cars.CountAsync(c => c.BrandId == car.BrandId && c.IsApproved && !c.IsDeleted);
            }

            await _db.SaveChangesAsync();

            TempData["Success"] = $"\"{car.Title}\" zibil qutusuna göndərildi.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Zibil qutusundan bərpa et
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreCar(int id)
        {
            var car = await _db.Cars.FindAsync(id);
            if (car == null) return NotFound();

            car.IsDeleted = false;
            car.DeletedDate = null;

            await _db.SaveChangesAsync();

            TempData["Success"] = $"\"{car.Title}\" bərpa edildi.";
            return RedirectToAction("Trash", "SellCarRequest");
        }

        // POST: Həmişəlik sil
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> PermanentDeleteCar(int id)
        {
            var car = await _db.Cars.Include(c => c.Images).FirstOrDefaultAsync(c => c.Id == id);
            if (car == null) return NotFound();

            if (!string.IsNullOrEmpty(car.ThumbnailUrl)) _fileService.Delete(car.ThumbnailUrl);
            foreach (var img in car.Images) _fileService.Delete(img.ImageUrl);

            var brandId = car.BrandId;
            _db.Cars.Remove(car);
            await _db.SaveChangesAsync();

            var brand = await _db.Brands.FindAsync(brandId);
            if (brand != null)
            {
                brand.VehicleCount = await _db.Cars.CountAsync(c => c.BrandId == brandId && c.IsApproved && !c.IsDeleted);
                await _db.SaveChangesAsync();
            }

            TempData["Success"] = "Maşın həmişəlik silindi.";
            return RedirectToAction("Trash", "SellCarRequest");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(int id)
        {
            var img = await _db.CarImages.FindAsync(id);
            if (img == null) return NotFound();
            _fileService.Delete(img.ImageUrl);
            _db.CarImages.Remove(img);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Şəkil silindi.";
            return RedirectToAction(nameof(Edit), new { id = img.CarId });
        }
    }
}
