using Car_Project.Models;

namespace Car_Project.ViewModels.Compare
{
    // Müqayis? üçün bir avtomobilin xüsusiyy?tl?ri
    public class CompareCarViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Year { get; set; }
        public int Mileage { get; set; }
        public string FuelType { get; set; } = string.Empty;
        public string Transmission { get; set; } = string.Empty;
        public string? BodyStyle { get; set; }
        public string? DriveType { get; set; }
        public string? Color { get; set; }
        public int? Cylinders { get; set; }
        public int? DoorCount { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public string Condition { get; set; } = string.Empty;
        public IList<string> Features { get; set; } = new List<string>();
    }

    // ?sas Compare s?hif?si ViewModel
    public class CompareIndexViewModel
    {
        public IList<CompareCarViewModel> Cars { get; set; } = new List<CompareCarViewModel>();

        // Modal üçün - seçil?c?k avtomobill?r
        public IList<CompareCarViewModel> AvailableCars { get; set; } = new List<CompareCarViewModel>();

        // Bütün mümkün xüsusiyy?tl?r siyah?s? (müqayis? c?dv?li üçün)
        public IList<string> AllFeatures { get; set; } = new List<string>();
    }
}
