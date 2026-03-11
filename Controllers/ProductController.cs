using Car_Project.Services.Abstractions;
using Car_Project.ViewModels.Shop;
using Microsoft.AspNetCore.Mvc;

namespace Car_Project.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICartService    _cartService;

        public ProductController(IProductService productService, ICartService cartService)
        {
            _productService = productService;
            _cartService    = cartService;
        }

        // GET /Product  veya  /Product/Index
        public async Task<IActionResult> Index(
            string? category = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string? sortBy = null,
            string? q = null,
            int page = 1)
        {
            const int pageSize = 12;

            var (products, totalCount) = await _productService.GetFilteredAsync(
                page, pageSize, category, minPrice, maxPrice, sortBy, q);

            var categories = await _productService.GetCategoriesAsync();

            var vm = new ShopIndexViewModel
            {
                Products = products.Select(p => new ProductCardViewModel
                {
                    Id           = p.Id,
                    Name         = p.Name,
                    Slug         = p.Slug,
                    Price        = p.Price,
                    OldPrice     = p.OldPrice,
                    ThumbnailUrl = p.ThumbnailUrl
                                   ?? p.Images.FirstOrDefault()?.ImageUrl,
                    Badge        = p.Badge,
                    BadgeColor   = p.BadgeColor,
                    CategoryName = p.Category?.Name ?? string.Empty,
                    InStock      = p.Stock > 0
                }).ToList(),

                Categories       = categories.Select(c => c.Name).ToList(),
                SelectedCategory = category,
                MinPrice         = minPrice,
                MaxPrice         = maxPrice,
                SortBy           = sortBy,
                SearchQuery      = q,
                CurrentPage      = page,
                TotalPages       = (int)Math.Ceiling(totalCount / (double)pageSize),
                TotalCount       = totalCount
            };

            return View(vm);
        }

        // GET /Product/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null) return NotFound();

            var related = await _productService.GetRelatedAsync(product.CategoryId, product.Id, 4);

            var vm = new ProductDetailViewModel
            {
                Id           = product.Id,
                Name         = product.Name,
                Description  = product.Description,
                Price        = product.Price,
                OldPrice     = product.OldPrice,
                Stock        = product.Stock,
                Badge        = product.Badge,
                BadgeColor   = product.BadgeColor,
                CategoryName = product.Category?.Name ?? string.Empty,
                Images       = product.Images.OrderBy(i => i.Order)
                                             .Select(i => i.ImageUrl)
                                             .ToList(),
                RelatedProducts = related.Select(r => new ProductCardViewModel
                {
                    Id           = r.Id,
                    Name         = r.Name,
                    Slug         = r.Slug,
                    Price        = r.Price,
                    OldPrice     = r.OldPrice,
                    ThumbnailUrl = r.ThumbnailUrl ?? r.Images.FirstOrDefault()?.ImageUrl,
                    Badge        = r.Badge,
                    BadgeColor   = r.BadgeColor,
                    CategoryName = product.Category?.Name ?? string.Empty,
                    InStock      = r.Stock > 0
                }).ToList()
            };

            return View(vm);
        }

        // POST /Product/AddToCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            // Touch the session so the cookie is committed and Session.Id is stable
            HttpContext.Session.SetString("_init", "1");
            var sessionId = HttpContext.Session.Id;

            try
            {
                await _cartService.AddToCartAsync(sessionId, productId, quantity);
            }
            catch (InvalidOperationException ex)
            {
                TempData["CartError"] = ex.Message;
                return RedirectToAction("Details", new { id = productId });
            }

            return RedirectToAction("Index", "Payment");
        }

        // POST /Product/RemoveFromCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            HttpContext.Session.SetString("_init", "1");
            var sessionId = HttpContext.Session.Id;
            await _cartService.RemoveFromCartAsync(sessionId, productId);
            return RedirectToAction("Index", "Payment");
        }

        // POST /Product/BuyNow
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BuyNow(int productId, int quantity = 1)
        {
            HttpContext.Session.SetString("_init", "1");
            var sessionId = HttpContext.Session.Id;

            try
            {
                await _cartService.AddToCartAsync(sessionId, productId, quantity);
            }
            catch (InvalidOperationException ex)
            {
                TempData["CartError"] = ex.Message;
                return RedirectToAction("Details", new { id = productId });
            }

            return RedirectToAction("Index", "Checkout");
        }
    }
}
