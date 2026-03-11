using Car_Project.Data;
using Car_Project.Models;
using Car_Project.Services.Abstractions;
using Car_Project.ViewModels.List;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Car_Project.Controllers
{
    public class ListController : Controller
    {
        private readonly ICarService _carService;
        private readonly IBrandService _brandService;
        private readonly ICarFeatureService _carFeatureService;
        private readonly IWishlistService _wishlistService;
        private readonly ICompareItemService _compareService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly INotificationService _notificationService;

        public ListController(
            ICarService carService,
            IBrandService brandService,
            ICarFeatureService carFeatureService,
            IWishlistService wishlistService,
            ICompareItemService compareService,
            ApplicationDbContext context,
            UserManager<AppUser> userManager,
            INotificationService notificationService)
        {
            _carService = carService;
            _brandService = brandService;
            _carFeatureService = carFeatureService;
            _wishlistService = wishlistService;
            _compareService = compareService;
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index(
            string? brand = null,
            string? condition = null,
            string? fuelType = null,
            string? transmission = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            int? minYear = null,
            int? maxYear = null,
            int page = 1,
            string? viewMode = "grid",
            string? sortBy = null,
            string? bodyStyle = null)
        {
            const int pageSize = 9;

            // Parse enums
            int? brandId = null;
            if (!string.IsNullOrEmpty(brand))
            {
                var brandObj = (await _brandService.GetAllAsync())
                    .FirstOrDefault(b => b.Name.Equals(brand, StringComparison.OrdinalIgnoreCase));
                brandId = brandObj?.Id;
            }

            CarCondition? conditionEnum = null;
            if (!string.IsNullOrEmpty(condition) && Enum.TryParse<CarCondition>(condition, true, out var c))
                conditionEnum = c;

            FuelType? fuelTypeEnum = null;
            if (!string.IsNullOrEmpty(fuelType) && Enum.TryParse<FuelType>(fuelType, true, out var f))
                fuelTypeEnum = f;

            TransmissionType? transmissionEnum = null;
            if (!string.IsNullOrEmpty(transmission) && Enum.TryParse<TransmissionType>(transmission, true, out var t))
                transmissionEnum = t;

            var filtered = await _carService.GetFilteredAsync(
                brandId, conditionEnum, fuelTypeEnum, transmissionEnum,
                minPrice, maxPrice, minYear, maxYear, bodyStyle);

            // ── Apply sorting ──────────────────────────────────────────────────
            IEnumerable<Car> sorted = sortBy switch
            {
                "lowest-price"   => filtered.OrderBy(car => car.Price),
                "highest-price"  => filtered.OrderByDescending(car => car.Price),
                "newest-year"    => filtered.OrderByDescending(car => car.Year),
                "lowest-mileage" => filtered.OrderBy(car => car.Mileage),
                _                => filtered.OrderByDescending(car => car.CreatedDate)
            };

            // Pagination
            var totalCount = filtered.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            if (page < 1) page = 1;
            var paged = sorted.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // Brands for filter panel
            var allBrands = await _brandService.GetAllAsync();

            var cars = paged.Select(car => new ListCarCardViewModel
            {
                Id = car.Id,
                Title = car.Title,
                Price = car.Price,
                MonthlyPayment = car.MonthlyPayment,
                Year = car.Year,
                Mileage = car.Mileage,
                FuelType = car.FuelType.ToString(),
                Transmission = car.Transmission.ToString(),
                ThumbnailUrl = car.Images.FirstOrDefault(i => i.IsMain)?.ImageUrl
                               ?? car.Images.FirstOrDefault()?.ImageUrl
                               ?? car.ThumbnailUrl,
                ImageUrls = car.Images.Select(i => i.ImageUrl).ToList(),
                Badge = car.Badge,
                BadgeColor = car.BadgeColor,
                BrandName = car.Brand?.Name ?? "",
                BodyStyle = car.BodyStyle,
                Condition = car.Condition,
                ImageCount = car.Images.Count,
                VideoCount = 0,
                IsVip = car.IsVip
            }).ToList();

            // Wishlist & Compare vəziyyəti
            var sessionId = HttpContext.Session.Id;
            var wishlistCars = await _wishlistService.GetWishlistAsync(sessionId);
            var compareCars = await _compareService.GetCompareListAsync(sessionId);

            var vm = new ListIndexViewModel
            {
                Cars = cars,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalCount = totalCount,
                ViewMode = viewMode ?? "grid",
                WishlistedCarIds = wishlistCars.Select(c => c.Id).ToHashSet(),
                ComparedCarIds = compareCars.Select(c => c.Id).ToHashSet(),
                Filter = new ListFilterViewModel
                {
                    Brands = allBrands.Select(b => b.Name).ToList(),
                    FuelTypes = Enum.GetNames<FuelType>().ToList(),
                    Transmissions = Enum.GetNames<TransmissionType>().ToList(),
                    SelectedBrand = brand,
                    SelectedFuelType = fuelType,
                    SelectedTransmission = transmission,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice,
                    MinYear = minYear ?? 2000,
                    MaxYear = maxYear ?? 2026,
                    Condition = condition,
                    SortBy = sortBy,
                    SelectedBodyStyle = bodyStyle
                }
            };

            return View(vm);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var car = await _carService.GetByIdAsync(id);
            if (car == null)
                return NotFound();

            var features = await _carFeatureService.GetByCarIdAsync(id);
            var relatedCars = await _carService.GetRelatedAsync(car.BrandId, car.Id, 4);

            // Load owner info
            AppUser? owner = null;
            if (!string.IsNullOrEmpty(car.UserId))
                owner = await _userManager.FindByIdAsync(car.UserId);

            // Load approved reviews for this car (only top-level, with replies)
            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Replies.Where(reply => reply.IsApproved))
                    .ThenInclude(reply => reply.User)
                .Where(r => r.CarId == id && r.IsApproved && r.ParentReviewId == null)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();

            var vm = new ListDetailViewModel
            {
                Id = car.Id,
                Title = car.Title,
                Price = car.Price,
                MonthlyPayment = car.MonthlyPayment,
                Year = car.Year,
                Mileage = car.Mileage,
                FuelType = car.FuelType.ToString(),
                Transmission = car.Transmission.ToString(),
                BodyStyle = car.BodyStyle,
                DriveType = car.DriveType,
                Color = car.Color,
                InteriorColor = car.InteriorColor,
                Cylinders = car.Cylinders,
                DoorCount = car.DoorCount,
                Description = car.Description,
                Badge = car.Badge,
                BadgeColor = car.BadgeColor,
                BrandName = car.Brand?.Name ?? "",
                Condition = car.Condition,
                ImageUrls = car.Images.Any()
                    ? car.Images.OrderBy(i => i.Order).Select(i => i.ImageUrl).ToList()
                    : (!string.IsNullOrEmpty(car.ThumbnailUrl)
                        ? new List<string> { car.ThumbnailUrl }
                        : new List<string>()),
                ImageCount = car.Images.Any() ? car.Images.Count : (!string.IsNullOrEmpty(car.ThumbnailUrl) ? 1 : 0),
                VideoCount = 0,

                // Features grouped by category (Exterior, Interior, Safety, etc.)
                Features = features.Select(f => f.Name).ToList(),

                // Car Reviews
                CarReviews = reviews.Select(r => new CarReviewViewModel
                {
                    Id = r.Id,
                    AuthorName = r.User?.FullName ?? r.AuthorName,
                    AuthorAvatarUrl = r.User?.AvatarUrl ?? r.AvatarUrl,
                    Content = r.Content,
                    Rating = r.Rating,
                    CreatedDate = r.CreatedDate,
                    Replies = r.Replies
                        .OrderBy(reply => reply.CreatedDate)
                        .Select(reply => new CarReviewViewModel
                        {
                            Id = reply.Id,
                            AuthorName = reply.User?.FullName ?? reply.AuthorName,
                            AuthorAvatarUrl = reply.User?.AvatarUrl ?? reply.AvatarUrl,
                            Content = reply.Content,
                            Rating = reply.Rating,
                            CreatedDate = reply.CreatedDate
                        }).ToList()
                }).ToList(),
                AverageRating = reviews.Any() ? Math.Round(reviews.Average(r => r.Rating), 1) : 0,
                TotalReviews = reviews.Count,

                // Related cars ("You might also like")
                RelatedCars = relatedCars.Select(c => new ListCarCardViewModel
                {
                    Id = c.Id,
                    Title = c.Title,
                    Price = c.Price,
                    MonthlyPayment = c.MonthlyPayment,
                    Year = c.Year,
                    Mileage = c.Mileage,
                    FuelType = c.FuelType.ToString(),
                    Transmission = c.Transmission.ToString(),
                    ThumbnailUrl = c.Images.FirstOrDefault(i => i.IsMain)?.ImageUrl
                                   ?? c.Images.FirstOrDefault()?.ImageUrl
                                   ?? c.ThumbnailUrl,
                    ImageUrls = c.Images.Select(i => i.ImageUrl).ToList(),
                    Badge = c.Badge,
                    BadgeColor = c.BadgeColor,
                    BrandName = c.Brand?.Name ?? "",
                    BodyStyle = c.BodyStyle,
                    Condition = c.Condition,
                    ImageCount = c.Images.Count,
                    VideoCount = 0,
                    IsVip = c.IsVip
                }).ToList(),

                // Owner info
                OwnerName = owner?.FullName,
                OwnerAvatarUrl = owner?.AvatarUrl,
                OwnerPhone = owner?.PhoneNumber,
                OwnerEmail = owner?.Email
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> AddReview(int carId, string content, int rating)
        {
            if (string.IsNullOrWhiteSpace(content) || rating < 1 || rating > 5)
            {
                TempData["ReviewError"] = "Rəy mətni və reytinq (1-5) tələb olunur.";
                return RedirectToAction(nameof(Detail), new { id = carId });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ReviewError"] = "İstifadəçi tapılmadı.";
                return RedirectToAction(nameof(Detail), new { id = carId });
            }

            var review = new Review
            {
                CarId = carId,
                UserId = user.Id,
                AuthorName = user.FullName ?? user.Email ?? "İstifadəçi",
                Content = content,
                Rating = rating,
                IsApproved = true,
                CreatedDate = DateTime.UtcNow
            };

            await _context.Reviews.AddAsync(review);
            await _context.SaveChangesAsync();

            // Maşın sahibinə bildiriş göndər
            var car = await _context.Cars.AsNoTracking().FirstOrDefaultAsync(c => c.Id == carId);
            if (car?.UserId != null && car.UserId != user.Id)
            {
                await _notificationService.CreateForUserAsync(
                    car.UserId,
                    "Yeni Rəy",
                    $"{user.FullName} \"{car.Title}\" elanınıza rəy yazdı.",
                    NotificationType.NewReview,
                    $"/List/Detail/{carId}");
            }

            TempData["ReviewSuccess"] = "Rəyiniz uğurla əlavə olundu!";
            return RedirectToAction(nameof(Detail), new { id = carId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ReplyToReview(int carId, int parentReviewId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["ReviewError"] = "Cavab mətni boş ola bilməz.";
                return RedirectToAction(nameof(Detail), new { id = carId });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ReviewError"] = "İstifadəçi tapılmadı.";
                return RedirectToAction(nameof(Detail), new { id = carId });
            }

            var parentReview = await _context.Reviews.FindAsync(parentReviewId);
            if (parentReview == null)
            {
                TempData["ReviewError"] = "Cavab verilən rəy tapılmadı.";
                return RedirectToAction(nameof(Detail), new { id = carId });
            }

            var reply = new Review
            {
                CarId = carId,
                UserId = user.Id,
                AuthorName = user.FullName ?? user.Email ?? "İstifadəçi",
                Content = content,
                Rating = parentReview.Rating,
                IsApproved = true,
                ParentReviewId = parentReviewId,
                CreatedDate = DateTime.UtcNow
            };

            await _context.Reviews.AddAsync(reply);
            await _context.SaveChangesAsync();

            // Rəy sahibinə bildiriş göndər
            if (parentReview.UserId != null && parentReview.UserId != user.Id)
            {
                await _notificationService.CreateForUserAsync(
                    parentReview.UserId,
                    "Rəyinizə Cavab",
                    $"{user.FullName} rəyinizə cavab yazdı.",
                    NotificationType.NewReview,
                    $"/List/Detail/{carId}");
            }

            TempData["ReviewSuccess"] = "Cavabınız uğurla əlavə olundu!";
            return RedirectToAction(nameof(Detail), new { id = carId });
        }
    }
}
