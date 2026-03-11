using Car_Project.Models;

namespace Car_Project.ViewModels.Home
{
    // Axtar?? filtri üçün ViewModel
    public class CarSearchFilterViewModel
    {
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public int? MinMileage { get; set; }
        public int? MaxMileage { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? MinYear { get; set; }
        public int? MaxYear { get; set; }
        public string? FuelType { get; set; }
        public string? Transmission { get; set; }
        public string? DriveType { get; set; }
        public string? Color { get; set; }
        public int? Cylinders { get; set; }
        public string? BodyStyle { get; set; }
        public IList<string> Features { get; set; } = new List<string>();
    }

    // Avtomobil kart? üçün kiçik ViewModel
    public class CarCardViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? MonthlyPayment { get; set; }
        public int Year { get; set; }
        public int Mileage { get; set; }
        public string FuelType { get; set; } = string.Empty;
        public string Transmission { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public string? Badge { get; set; }
        public string? BadgeColor { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public int ImageCount { get; set; }
        public int VideoCount { get; set; }
        public CarCondition Condition { get; set; }
    }

    // Brend kart? üçün ViewModel
    public class BrandCardViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public int VehicleCount { get; set; }
    }

    // Mü?t?ri r?yi üçün ViewModel
    public class ReviewCardViewModel
    {
        public string AuthorName { get; set; } = string.Empty;
        public string? AuthorTitle { get; set; }
        public string? AvatarUrl { get; set; }
        public string Content { get; set; } = string.Empty;
        public int Rating { get; set; }
    }

    // Blog yaz?s? kart? üçün ViewModel (Home s?hif?si üçün sad? versiya)
    public class NewsCardViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public string? AuthorName { get; set; }
        public DateTime? PublishedAt { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
    }

    // ?sas Home s?hif?si ViewModel
    public class HomeIndexViewModel
    {
        public CarSearchFilterViewModel Filter { get; set; } = new();
        public IList<string> BrandNames { get; set; } = new List<string>();
        public IList<string> FuelTypes { get; set; } = new List<string>();
        public IList<string> Transmissions { get; set; } = new List<string>();
        public IList<CarCardViewModel> NewCars { get; set; } = new List<CarCardViewModel>();
        public IList<CarCardViewModel> UsedCars { get; set; } = new List<CarCardViewModel>();
        public IList<CarCardViewModel> TrendingCars { get; set; } = new List<CarCardViewModel>();
        public IList<BrandCardViewModel> Brands { get; set; } = new List<BrandCardViewModel>();
        public IList<ReviewCardViewModel> Reviews { get; set; } = new List<ReviewCardViewModel>();
        public IList<NewsCardViewModel> LatestNews { get; set; } = new List<NewsCardViewModel>();
        public int TotalMatchCount { get; set; }
    }
}
