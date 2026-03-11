namespace Car_Project.Models
{
    public class CompareItem : BaseEntity
    {
        public string SessionId { get; set; } = string.Empty;

        public int CarId { get; set; }
        public Car Car { get; set; } = null!;
    }
}
