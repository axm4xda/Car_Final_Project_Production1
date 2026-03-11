using Car_Project.Services.Abstractions;
using Car_Project.ViewModels.Payment;
using Car_Project.ViewModels.Shop;
using Microsoft.AspNetCore.Mvc;

namespace Car_Project.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ICartService    _cartService;
        private readonly IPaymentService _paymentService;
        private readonly IProductService _productService;

        public PaymentController(
            ICartService    cartService,
            IPaymentService paymentService,
            IProductService productService)
        {
            _cartService    = cartService;
            _paymentService = paymentService;
            _productService = productService;
        }

        // GET /Payment  (Shopping Cart səhifəsi)
        public async Task<IActionResult> Index()
        {
            HttpContext.Session.SetString("_init", "1");
            var sessionId = HttpContext.Session.Id;
            var cartItems = await _cartService.GetCartAsync(sessionId);

            var featured = await _productService.GetFeaturedAsync(4);

            var vm = new CartViewModel
            {
                Items = cartItems.Select(ci => new CartItemViewModel
                {
                    ProductId    = ci.ProductId,
                    ProductName  = ci.Product.Name,
                    ThumbnailUrl = ci.Product.ThumbnailUrl
                                   ?? ci.Product.Images.FirstOrDefault()?.ImageUrl,
                    UnitPrice    = ci.Product.Price,
                    Quantity     = ci.Quantity
                }).ToList()
            };

            ViewBag.RelatedProducts = featured.Select(p => new ProductCardViewModel
            {
                Id           = p.Id,
                Name         = p.Name,
                Slug         = p.Slug,
                Price        = p.Price,
                OldPrice     = p.OldPrice,
                ThumbnailUrl = p.ThumbnailUrl ?? p.Images.FirstOrDefault()?.ImageUrl,
                Badge        = p.Badge,
                BadgeColor   = p.BadgeColor,
                CategoryName = p.Category?.Name ?? string.Empty,
                InStock      = p.Stock > 0
            }).ToList();

            return View(vm);
        }

        // POST /Payment/UpdateQuantity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int productId, int quantity)
        {
            HttpContext.Session.SetString("_init", "1");
            var sessionId = HttpContext.Session.Id;
            if (quantity < 1)
                await _cartService.RemoveFromCartAsync(sessionId, productId);
            else
                await _cartService.UpdateQuantityAsync(sessionId, productId, quantity);

            return RedirectToAction(nameof(Index));
        }

        // POST /Payment/Remove
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int productId)
        {
            HttpContext.Session.SetString("_init", "1");
            var sessionId = HttpContext.Session.Id;
            await _cartService.RemoveFromCartAsync(sessionId, productId);
            return RedirectToAction(nameof(Index));
        }

        // GET /Payment/Result?paymentId=3
        public async Task<IActionResult> Result(int paymentId)
        {
            var payment = await _paymentService.GetByIdAsync(paymentId);
            if (payment == null) return RedirectToAction(nameof(Index));

            var vm = new PaymentResultViewModel
            {
                IsSuccess     = payment.Status == Car_Project.Models.PaymentStatus.Paid,
                TransactionId = payment.TransactionId,
                OrderId       = payment.Order?.Id ?? 0,
                Amount        = payment.Amount,
                Method        = payment.Method,
                PaidAt        = payment.PaidAt ?? payment.CreatedDate,
                ErrorMessage  = payment.Status == Car_Project.Models.PaymentStatus.Failed
                                    ? "Ödəniş uğursuz oldu." : null
            };

            return View(vm);
        }
    }
}
