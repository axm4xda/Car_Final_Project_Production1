namespace Car_Project.Models
{
    public enum FuelType
    {
        Petrol,
        Diesel,
        Electric,
        Hybrid
    }

    public enum TransmissionType
    {
        Manual,
        Automatic
    }

    public enum CarCondition
    {
        New,
        Used
    }

    public enum ListingType
    {
        Normal = 0,
        VIP    = 1
    }

    public class Car : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? MonthlyPayment { get; set; }
        public int Year { get; set; }
        public int Mileage { get; set; }
        public FuelType FuelType { get; set; }
        public TransmissionType Transmission { get; set; }
        public CarCondition Condition { get; set; }
        public string? BodyStyle { get; set; }
        public string? DriveType { get; set; }
        public string? Color { get; set; }
        public string? InteriorColor { get; set; }
        public int? Cylinders { get; set; }
        public int? DoorCount { get; set; }
        public string? Description { get; set; }

        /// <summary>Birbaşa thumbnail URL — CarImage cədvəlindən müstəqil fallback şəkil</summary>
        public string? ThumbnailUrl { get; set; }

        /// <summary>"Special", "Great Price" kimi badge mətni</summary>
        public string? Badge { get; set; }

        /// <summary>"bg-primary-2", "bg-green" kimi CSS sinfi</summary>
        public string? BadgeColor { get; set; }

        /// <summary>Admin tərəfindən təsdiqlənib-təsdiqlənmədiyi</summary>
        public bool IsApproved { get; set; }

        /// <summary>Elanı paylaşan istifadəçinin ID-si</summary>
        public string? UserId { get; set; }
        public AppUser? User { get; set; }

        public int BrandId { get; set; }
        public Brand Brand { get; set; } = null!;

        /// <summary>Elan növü: Normal (pulsuz) və ya VIP (ödənişli, üstdə göstərilir)</summary>
        public ListingType ListingType { get; set; } = ListingType.Normal;

        /// <summary>VIP ödənişinin tamamlandığı tarix</summary>
        public DateTime? VipPaidAt { get; set; }

        /// <summary>VIP elan olub-olmadığını göstərir</summary>
        public bool IsVip => ListingType == ListingType.VIP;

        /// <summary>Soft delete — zibil qutusuna atılıb</summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>Zibil qutusuna atılma tarixi</summary>
        public DateTime? DeletedDate { get; set; }

        public ICollection<CarImage> Images { get; set; } = new List<CarImage>();
        public ICollection<CarFeatureMapping> Features { get; set; } = new List<CarFeatureMapping>();
    }
}
