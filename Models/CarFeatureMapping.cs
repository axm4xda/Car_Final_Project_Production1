namespace Car_Project.Models
{
    /// <summary>Car — CarFeature many-to-many ?laq? c?dv?li</summary>
    public class CarFeatureMapping
    {
        public int CarId { get; set; }
        public Car Car { get; set; } = null!;

        public int CarFeatureId { get; set; }
        public CarFeature CarFeature { get; set; } = null!;
    }
}
