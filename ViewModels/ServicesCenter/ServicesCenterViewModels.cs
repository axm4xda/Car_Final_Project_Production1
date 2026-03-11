namespace Car_Project.ViewModels.ServicesCenter
{
    // Servis m?rk?zi kart? üçün ViewModel
    public class ServiceCenterCardViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? ImageUrl { get; set; }
        public string? WorkingHours { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    // ?sas ServicesCenter s?hif?si ViewModel
    public class ServicesCenterIndexViewModel
    {
        public IList<ServiceCenterCardViewModel> ServiceCenters { get; set; } = new List<ServiceCenterCardViewModel>();

        // Axtar??
        public string? SearchQuery { get; set; }

        // S?hif?l?m?
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
    }
}
