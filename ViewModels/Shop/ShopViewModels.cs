using Car_Project.Models;

namespace Car_Project.ViewModels.Shop
{
    public class ProductCardViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Slug { get; set; }
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? Badge { get; set; }
        public string? BadgeColor { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public bool InStock { get; set; }
    }

    public class ProductDetailViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public int Stock { get; set; }
        public string? Badge { get; set; }
        public string? BadgeColor { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public IList<string> Images { get; set; } = new List<string>();
        public IList<ProductCardViewModel> RelatedProducts { get; set; } = new List<ProductCardViewModel>();
    }

    public class ShopIndexViewModel
    {
        public IList<ProductCardViewModel> Products { get; set; } = new List<ProductCardViewModel>();
        public IList<string> Categories { get; set; } = new List<string>();

        // Filtrasiya
        public string? SelectedCategory { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? SortBy { get; set; }
        public string? SearchQuery { get; set; }

        // S?hif?l?m?
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
    }

    public class CartItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => UnitPrice * Quantity;
    }

    public class CartViewModel
    {
        public IList<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();
        public decimal SubTotal => Items.Sum(i => i.TotalPrice);
        public int TotalItems => Items.Sum(i => i.Quantity);
    }
}
