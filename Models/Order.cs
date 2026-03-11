namespace Car_Project.Models
{
    public enum OrderStatus
    {
        Pending,
        Processing,
        Shipped,
        Delivered,
        Cancelled,
        Refunded
    }

    public enum PaymentMethod
    {
        CreditCard,
        CashOnDelivery,
        ApplePay,
        PayPal
    }

    public enum PaymentStatus
    {
        Pending,
        Paid,
        Failed,
        Refunded
    }

    public class Order : BaseEntity
    {
        // Billing məlumatları
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? Street { get; set; }
        public string? State { get; set; }
        public string PostalCode { get; set; } = string.Empty;
        public string? Note { get; set; }

        // Məbləğlər
        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal Total { get; set; }
        public string? CouponCode { get; set; }

        // Status
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        // Ödəniş
        public int PaymentId { get; set; }
        public Payment Payment { get; set; } = null!;

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }

    public class OrderItem : BaseEntity
    {
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
    }

    public class Payment : BaseEntity
    {
        public PaymentMethod Method { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        /// <summary>Kart sahibinin adı (kartla ödənişdə)</summary>
        public string? CardHolderName { get; set; }

        /// <summary>Kart nömrəsinin son 4 rəqəmi (tam saxlanmır)</summary>
        public string? CardLastFour { get; set; }

        public string? TransactionId { get; set; }
        public DateTime? PaidAt { get; set; }
        public decimal Amount { get; set; }

        public Order? Order { get; set; }
    }

    public class Coupon : BaseEntity
    {
        public string Code { get; set; } = string.Empty;

        /// <summary>Faiz endirim (məs: 10 = 10%)</summary>
        public decimal DiscountPercent { get; set; }

        /// <summary>Sabit məbləğ endirimi (məs: 20 = $20 endirim)</summary>
        public decimal? DiscountAmount { get; set; }

        /// <summary>Kuponu tətbiq etmək üçün minimum sifariş məbləği</summary>
        public decimal MinOrderAmount { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime? ExpiresAt { get; set; }
        public int UsageLimit { get; set; }
        public int UsedCount { get; set; }

        /// <summary>Kuponu alan istifadəçinin Id-si (null = hamı üçün)</summary>
        public string? UserId { get; set; }
    }
}
