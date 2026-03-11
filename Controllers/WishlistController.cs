using Car_Project.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Car_Project.Controllers
{
    public class WishlistController : Controller
    {
        private readonly IWishlistService _wishlistService;

        public WishlistController(IWishlistService wishlistService)
        {
            _wishlistService = wishlistService;
        }

        private string SessionId => HttpContext.Session.Id;

        // GET /Wishlist
        public async Task<IActionResult> Index()
        {
            var cars = await _wishlistService.GetWishlistAsync(SessionId);
            return View(cars);
        }

        // POST /Wishlist/Toggle/5
        [HttpPost]
        public async Task<IActionResult> Toggle(int id)
        {
            bool isNow;
            if (await _wishlistService.IsInWishlistAsync(SessionId, id))
            {
                await _wishlistService.RemoveAsync(SessionId, id);
                isNow = false;
            }
            else
            {
                await _wishlistService.AddAsync(SessionId, id);
                isNow = true;
            }

            var count = await _wishlistService.GetCountAsync(SessionId);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true, isInWishlist = isNow, count });

            return RedirectToAction("Index");
        }

        // POST /Wishlist/Remove/5
        [HttpPost]
        public async Task<IActionResult> Remove(int id)
        {
            await _wishlistService.RemoveAsync(SessionId, id);
            var count = await _wishlistService.GetCountAsync(SessionId);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true, count });

            return RedirectToAction("Index");
        }

        // POST /Wishlist/Clear
        [HttpPost]
        public async Task<IActionResult> Clear()
        {
            await _wishlistService.ClearAsync(SessionId);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true, count = 0 });

            return RedirectToAction("Index");
        }

        // GET /Wishlist/Count
        [HttpGet]
        public async Task<IActionResult> Count()
        {
            var count = await _wishlistService.GetCountAsync(SessionId);
            return Json(new { count });
        }
    }
}
