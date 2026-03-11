using Car_Project.Models;

namespace Car_Project.ViewModels.List
{
    // Avtomobil kartı için ViewModel
    public class ListCarCardViewModel
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
        public IList<string> ImageUrls { get; set; } = new List<string>();
        public string? Badge { get; set; }
        public string? BadgeColor { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public string? BodyStyle { get; set; }
        public CarCondition Condition { get; set; }
        public int ImageCount { get; set; }
        public int VideoCount { get; set; }

        /// <summary>VIP elan olub-olmadığı</summary>
        public bool IsVip { get; set; }
    }

    // Filtr paneli üçün ViewModel
    public class ListFilterViewModel
    {
        public IList<string> Brands { get; set; } = new List<string>();
        public IList<string> BodyStyles { get; set; } = new List<string>();
        public IList<string> FuelTypes { get; set; } = new List<string>();
        public IList<string> Transmissions { get; set; } = new List<string>();
        public IList<string> DriveTypes { get; set; } = new List<string>();
        public IList<string> Colors { get; set; } = new List<string>();
        public IList<int> CylinderOptions { get; set; } = new List<int>();
        public IList<string> AvailableFeatures { get; set; } = new List<string>();

        // Seçilmi? filtrl?r
        public string? SelectedBrand { get; set; }
        public string? SelectedBodyStyle { get; set; }
        public string? SelectedFuelType { get; set; }
        public string? SelectedTransmission { get; set; }
        public string? SelectedDriveType { get; set; }
        public string? SelectedColor { get; set; }
        public int? SelectedCylinders { get; set; }
        public IList<string> SelectedFeatures { get; set; } = new List<string>();
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? MinMileage { get; set; }
        public int? MaxMileage { get; set; }
        public int MinYear { get; set; } = 2000;
        public int MaxYear { get; set; } = 2026;
        public string? SearchQuery { get; set; }
        public string? SortBy { get; set; }
        public string? Condition { get; set; }
    }

    // ?sas List s?hif?si ViewModel
    public class ListIndexViewModel
    {
        public IList<ListCarCardViewModel> Cars { get; set; } = new List<ListCarCardViewModel>();
        public ListFilterViewModel Filter { get; set; } = new();

        // S?hif?l?m?
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }

        // Görünü? rejimi: "grid" v? ya "list"
        public string ViewMode { get; set; } = "grid";

        // Wishlist v? Compare v?ziyy?ti (card-larda active class üçün)
        public ISet<int> WishlistedCarIds { get; set; } = new HashSet<int>();
        public ISet<int> ComparedCarIds { get; set; } = new HashSet<int>();
    }

    // List Detail s?hif?si ???n ViewModel
    public class ListDetailViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? MonthlyPayment { get; set; }
        public int Year { get; set; }
        public int Mileage { get; set; }
        public string FuelType { get; set; } = string.Empty;
        public string Transmission { get; set; } = string.Empty;
        public string? BodyStyle { get; set; }
        public string? DriveType { get; set; }
        public string? Color { get; set; }
        public string? InteriorColor { get; set; }
        public int? Cylinders { get; set; }
        public int? DoorCount { get; set; }
        public string? Description { get; set; }
        public string? Badge { get; set; }
        public string? BadgeColor { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public CarCondition Condition { get; set; }
        public IList<string> ImageUrls { get; set; } = new List<string>();
        public int ImageCount { get; set; }
        public int VideoCount { get; set; }

        // Features grouped by category (Exterior, Interior, Safety, etc.)
        public IList<string> Features { get; set; } = new List<string>();

        // Related cars ("You might also like")
        public IList<ListCarCardViewModel> RelatedCars { get; set; } = new List<ListCarCardViewModel>();

        // ── Review bölməsi ────────────────────────────────────────────────────
        public IList<CarReviewViewModel> CarReviews { get; set; } = new List<CarReviewViewModel>();
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }

        public int GetStarPercent(int star)
        {
            if (TotalReviews == 0) return 0;
            int count = CarReviews.Count(r => r.Rating == star);
            return (int)Math.Round(count * 100.0 / TotalReviews);
        }

        // ── Car Owner (lister) info ───────────────────────────────────────────
        public string? OwnerName { get; set; }
        public string? OwnerAvatarUrl { get; set; }
        public string? OwnerPhone { get; set; }
        public string? OwnerEmail { get; set; }
    }

    // Car detail səhifəsindəki review kartı
    public class CarReviewViewModel
    {
        public int Id { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string? AuthorAvatarUrl { get; set; }
        public string Content { get; set; } = string.Empty;
        public int Rating { get; set; }
        public DateTime CreatedDate { get; set; }
        public IList<CarReviewViewModel> Replies { get; set; } = new List<CarReviewViewModel>();
    }
}
