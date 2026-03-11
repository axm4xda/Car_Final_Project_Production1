namespace Car_Project.Models
{
    public class Brand : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public int VehicleCount { get; set; }

        public ICollection<Car> Cars { get; set; } = new List<Car>();
    }
}
