namespace Car_Project.Models
{
    public class ProductCategory : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Slug { get; set; }
        public string? IconUrl { get; set; }

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }

    public class Product : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Slug { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public int Stock { get; set; }
        public string? ThumbnailUrl { get; set; }

        /// <summary>"New", "Sale", "Hot" kimi badge</summary>
        public string? Badge { get; set; }
        public string? BadgeColor { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsFeatured { get; set; }

        public int CategoryId { get; set; }
        public ProductCategory Category { get; set; } = null!;

        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }

    public class ProductImage : BaseEntity
    {
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsMain { get; set; }
        public int Order { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
    }

    /// <summary>Session-əsaslı səbət elementi</summary>
    public class CartItem : BaseEntity
    {
        public string SessionId { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
    }
}
