namespace Car_Project.Models
{
    public class CarFeature : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        public ICollection<CarFeatureMapping> Cars { get; set; } = new List<CarFeatureMapping>();
    }
}
