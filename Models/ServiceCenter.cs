namespace Car_Project.Models
{
    public class ServiceCenter : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? ImageUrl { get; set; }
        public string? WorkingHours { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
