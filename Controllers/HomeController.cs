using System.Diagnostics;
using Car_Project.Models;
using Car_Project.Services.Abstractions;
using Car_Project.ViewModels.Home;
using Microsoft.AspNetCore.Mvc;

namespace Car_Project.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICarService _carService;
        private readonly IBrandService _brandService;
        private readonly IReviewService _reviewService;
        private readonly IBlogService _blogService;

        public HomeController(
            ICarService carService,
            IBrandService brandService,
            IReviewService reviewService,
            IBlogService blogService)
        {
            _carService   = carService;
            _brandService = brandService;
            _reviewService = reviewService;
            _blogService  = blogService;
        }

        public async Task<IActionResult> Index()
        {
            var allCars     = await _carService.GetAllAsync(); // servis art?q CreatedDate DESC qaytar?r
            var trendingRaw = await _carService.GetTrendingAsync(6);

            var newCars = allCars
                .Where(c => c.Condition == CarCondition.New)
                .Take(8)
                .Select(MapToCarCard)
                .ToList();

            var trendingCars = trendingRaw
                .Select(MapToCarCard)
                .ToList();

            var brands = (await _brandService.GetAllAsync())
                .Select(b => new BrandCardViewModel
                {
                    Id           = b.Id,
                    Name         = b.Name,
                    LogoUrl      = b.LogoUrl,
                    VehicleCount = b.Cars.Count > 0 ? b.Cars.Count : b.VehicleCount
                })
                .ToList();

            var reviews = (await _reviewService.GetTopRatedAsync(8))
                .Select(r => new ReviewCardViewModel
                {
                    AuthorName  = r.AuthorName,
                    AuthorTitle = r.AuthorTitle,
                    AvatarUrl   = r.AvatarUrl,
                    Content     = r.Content,
                    Rating      = r.Rating
                })
                .ToList();

            var (blogPosts, _) = await _blogService.GetPublishedAsync(1, 3);
            var latestNews = blogPosts
                .Select(p => new NewsCardViewModel
                {
                    Id           = p.Id,
                    Title        = p.Title,
                    Slug         = p.Slug,
                    ThumbnailUrl = p.ThumbnailUrl,
                    AuthorName   = p.AuthorName,
                    PublishedAt  = p.PublishedAt,
                    CategoryName = p.Category?.Name ?? ""
                })
                .ToList();

            var brandNames  = brands.Select(b => b.Name).ToList();
            var fuelTypes    = Enum.GetNames<FuelType>().ToList();
            var transmissions = Enum.GetNames<TransmissionType>().ToList();

            var viewModel = new HomeIndexViewModel
            {
                NewCars         = newCars,
                UsedCars        = new List<CarCardViewModel>(), // bo? — view-da Used tab yoxdur
                TrendingCars    = trendingCars,
                Brands          = brands,
                Reviews         = reviews,
                LatestNews      = latestNews,
                BrandNames      = brandNames,
                FuelTypes       = fuelTypes,
                Transmissions   = transmissions,
                TotalMatchCount = allCars.Count
            };

            return View(viewModel);
        }

        private static CarCardViewModel MapToCarCard(Car c) => new()
        {
            Id             = c.Id,
            Title          = c.Title,
            Price          = c.Price,
            MonthlyPayment = c.MonthlyPayment,
            Year           = c.Year,
            Mileage        = c.Mileage,
            FuelType       = c.FuelType.ToString(),
            Transmission   = c.Transmission.ToString(),
            ThumbnailUrl   = c.Images.FirstOrDefault(i => i.IsMain)?.ImageUrl
                             ?? c.Images.FirstOrDefault()?.ImageUrl
                             ?? c.ThumbnailUrl,
            Badge          = c.Badge,
            BadgeColor     = c.BadgeColor,
            BrandName      = c.Brand?.Name ?? "",
            ImageCount     = c.Images.Count,
            VideoCount     = 0,
            Condition      = c.Condition
        };
    }
}
