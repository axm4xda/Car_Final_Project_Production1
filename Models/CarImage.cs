namespace Car_Project.Models
{
    public class CarImage : BaseEntity
    {
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsMain { get; set; }
        public int Order { get; set; }

        public int CarId { get; set; }
        public Car Car { get; set; } = null!;
    }
}
