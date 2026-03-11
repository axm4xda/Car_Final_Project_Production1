using Car_Project.Data;
using Car_Project.Models;
using Car_Project.Services.Abstractions;
using Car_Project.ViewModels.AddListing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Car_Project.Controllers
{
    [Authorize]
    public class AddListingController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IFileService _fileService;
        private readonly UserManager<AppUser> _userManager;
        private readonly INotificationService _notificationService;
        private readonly IPaymentService _paymentService;
        private readonly IEmailService _emailService;
        private readonly ILogger<AddListingController> _logger;

        // Fixed VIP listing fee
        private const decimal VipFee = 9.99m;

        // Allowed image extensions
        private static readonly HashSet<string> _allowedExtensions =
            new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };

        public AddListingController(
            ApplicationDbContext db,
            IFileService fileService,
            UserManager<AppUser> userManager,
            INotificationService notificationService,
            IPaymentService paymentService,
            IEmailService emailService,
            ILogger<AddListingController> logger)
        {
            _db = db;
            _fileService = fileService;
            _userManager = userManager;
            _notificationService = notificationService;
            _paymentService = paymentService;
            _emailService = emailService;
            _logger = logger;
        }

        // GET /AddListing
        public async Task<IActionResult> Index()
        {
            ViewBag.Brands = new SelectList(
                await _db.Brands.OrderBy(b => b.Name).ToListAsync(), "Id", "Name");
            ViewBag.AllFeatures = await _db.CarFeatures.OrderBy(f => f.Name).ToListAsync();

            // Tell the view whether the first listing is still free
            var userId = _userManager.GetUserId(User);
            var existingCount = await _db.Cars.CountAsync(c => c.UserId == userId);
            ViewBag.IsFirstListing = existingCount == 0;

            return View();
        }

        // POST /AddListing
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(
            Car car,
            IFormFile? thumbnailFile,
            List<IFormFile>? imageFiles,
            int[]? featureIds)
        {
            var userId = _userManager.GetUserId(User);

            // Reload ViewBag
            ViewBag.Brands = new SelectList(
                await _db.Brands.OrderBy(b => b.Name).ToListAsync(), "Id", "Name", car.BrandId);
            ViewBag.AllFeatures = await _db.CarFeatures.OrderBy(f => f.Name).ToListAsync();
            var existingCount = await _db.Cars.CountAsync(c => c.UserId == userId);
            ViewBag.IsFirstListing = existingCount == 0;

            // Validate thumbnail extension
            if (thumbnailFile != null)
            {
                var ext = Path.GetExtension(thumbnailFile.FileName);
                if (!_allowedExtensions.Contains(ext))
                {
                    TempData["AuthError"] = $"Əsas şəkil üçün yalnız JPG, JPEG, PNG və WEBP formatları qəbul edilir.";
                    return View(car);
                }
            }

            // Validate extra images
            if (imageFiles != null)
            {
                foreach (var img in imageFiles)
                {
                    var ext = Path.GetExtension(img.FileName);
                    if (!_allowedExtensions.Contains(ext))
                    {
                        TempData["AuthError"] = $"'{img.FileName}' faylı üçün yalnız JPG, JPEG, PNG və WEBP qəbul edilir.";
                        return View(car);
                    }
                }
            }

            // ── Payment rule check ────────────────────────────────────────────
            // First listing is free (Normal or VIP), every subsequent one requires payment.
            bool requiresPayment = existingCount > 0;

            // Always require payment for VIP, even first listing
            if (car.ListingType == ListingType.VIP)
                requiresPayment = true;

            car.CreatedDate = DateTime.UtcNow;
            car.IsApproved  = false;
            car.UserId      = userId;

            // If Normal listing AND NOT the first (requiresPayment=true, not VIP)
            // we treat it the same as VIP — redirect to payment.
            if (requiresPayment && car.ListingType == ListingType.Normal)
            {
                // Normal paid listing – still costs a (smaller) fee; reuse same payment page
                // We just set the listing type to Normal but still need payment.
            }

            // Upload thumbnail
            try
            {
                if (thumbnailFile != null)
                    car.ThumbnailUrl = await _fileService.UploadAsync(thumbnailFile, "uploads/cars");
            }
            catch (InvalidOperationException ex)
            {
                TempData["AuthError"] = ex.Message;
                return View(car);
            }

            // Save the car first (unpublished / not approved)
            _db.Cars.Add(car);
            await _db.SaveChangesAsync();

            // Upload extra images
            if (imageFiles is { Count: > 0 })
            {
                int imgOrder = 0;
                foreach (var f in imageFiles)
                {
                    try
                    {
                        var url = await _fileService.UploadAsync(f, "uploads/cars");
                        _db.CarImages.Add(new CarImage
                        {
                            CarId      = car.Id,
                            ImageUrl   = url,
                            Order      = imgOrder++,
                            IsMain     = imgOrder == 1,
                            CreatedDate = DateTime.UtcNow
                        });
                    }
                    catch (InvalidOperationException ex)
                    {
                        TempData["AuthError"] = ex.Message;
                    }
                }
            }

            // Save features
            if (featureIds != null)
            {
                foreach (var fid in featureIds)
                    _db.CarFeatureMappings.Add(new CarFeatureMapping { CarId = car.Id, CarFeatureId = fid });
            }

            await _db.SaveChangesAsync();

            // ── If payment is required, redirect to VIP/Payment page ─────────
            if (requiresPayment)
            {
                decimal fee = car.ListingType == ListingType.VIP ? VipFee : VipFee; // same fee for now
                TempData["PendingCarId"]    = car.Id;
                TempData["PendingCarTitle"] = car.Title;
                TempData["PendingFee"]      = fee.ToString("0.00");
                return RedirectToAction("VipPayment");
            }

            // ── Free first listing ────────────────────────────────────────────
            await NotifyAdminsAsync(car);
            TempData["AuthSuccess"] = "Elanınız uğurla göndərildi! Admin təsdiqindən sonra yayımlanacaq.";
            return RedirectToAction("Index", "Home");
        }

        // GET /AddListing/VipPayment
        public IActionResult VipPayment()
        {
            if (TempData["PendingCarId"] == null)
                return RedirectToAction("Index");

            var vm = new VipPaymentViewModel
            {
                CarId    = (int)TempData["PendingCarId"]!,
                CarTitle = TempData["PendingCarTitle"]?.ToString() ?? "",
                VipFee   = decimal.TryParse(TempData["PendingFee"]?.ToString(), out var f) ? f : VipFee
            };

            // Keep TempData for a POST re-display
            TempData.Keep("PendingCarId");
            TempData.Keep("PendingCarTitle");
            TempData.Keep("PendingFee");

            return View(vm);
        }

        // POST /AddListing/VipPayment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VipPayment(VipPaymentViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var car = await _db.Cars.FindAsync(vm.CarId);
            if (car == null || car.UserId != _userManager.GetUserId(User))
            {
                TempData["AuthError"] = "Elan tapılmadı.";
                return RedirectToAction("Index");
            }

            var payment = new Payment
            {
                Method         = PaymentMethod.CreditCard,
                CardHolderName = vm.CardHolderName,
                CardLastFour   = vm.CardNumber.Length >= 4 ? vm.CardNumber[^4..] : vm.CardNumber,
                Amount         = vm.VipFee
            };

            var processed = await _paymentService.ProcessAsync(payment);

            if (processed.Status != PaymentStatus.Paid)
            {
                ModelState.AddModelError("", "Ödəniş uğursuz oldu. Kart məlumatlarını yoxlayın.");
                return View(vm);
            }

            car.ListingType = ListingType.VIP;
            car.VipPaidAt   = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            await NotifyAdminsAsync(car);

            // ── VIP ödəniş təsdiq maili göndər ──────────────────────────────
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null && !string.IsNullOrWhiteSpace(currentUser.Email))
            {
                try
                {
                    await _emailService.SendVipListingPaymentAsync(
                        currentUser.Email,
                        currentUser.FullName,
                        car.Title,
                        processed.Amount,
                        processed.TransactionId,
                        processed.PaidAt ?? DateTime.UtcNow);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "VIP listing payment email failed for {Email}", currentUser.Email);
                }
            }

            TempData["AuthSuccess"] = car.ListingType == ListingType.VIP
                ? "VIP elanınız uğurla göndərildi! Admin təsdiqindən sonra siyahının başında göstəriləcək. ⭐"
                : "Elanınız uğurla göndərildi! Admin təsdiqindən sonra yayımlanacaq.";

            return RedirectToAction("Index", "Home");
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private async Task NotifyAdminsAsync(Car car)
        {
            // Update brand vehicle count
            var brand = await _db.Brands.FindAsync(car.BrandId);
            if (brand != null)
                brand.VehicleCount = await _db.Cars.CountAsync(c => c.BrandId == car.BrandId && c.IsApproved);
            await _db.SaveChangesAsync();

            var currentUser = await _userManager.GetUserAsync(User);
            var userName = currentUser?.FullName ?? "İstifadəçi";
            var vipNote  = car.IsVip ? " [VIP]" : "";

            await _notificationService.CreateForAdminsAsync(
                "Yeni Elan Əlavə Edildi",
                $"{userName} tərəfindən \"{car.Title}\"{vipNote} adlı yeni elan əlavə edildi. Təsdiq gözləyir.",
                NotificationType.NewCarListing,
                "/Admin/Car/Index");
        }
    }
}
