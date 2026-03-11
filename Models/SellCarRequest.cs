namespace Car_Project.Models
{
    public enum SellCarRequestStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        Trashed = 3
    }

    public class SellCarRequest : BaseEntity
    {
        public string OwnerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string CarTitle { get; set; } = string.Empty;
        public int Year { get; set; }
        public int Mileage { get; set; }
        public string? FuelType { get; set; }
        public string? Transmission { get; set; }
        public decimal AskingPrice { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsReviewed { get; set; }

        /// <summary>Müraciətin statusu: Pending, Approved, Rejected, Trashed</summary>
        public SellCarRequestStatus Status { get; set; } = SellCarRequestStatus.Pending;

        /// <summary>Admin qeydi (təsdiq/rədd səbəbi)</summary>
        public string? AdminNote { get; set; }

        /// <summary>Zibil qutusuna atılma tarixi (10 gün sonra avtomatik silinir)</summary>
        public DateTime? TrashedDate { get; set; }

        /// <summary>Təsdiq edildikdə yaradılan Car-ın Id-si</summary>
        public int? ApprovedCarId { get; set; }
    }
}
