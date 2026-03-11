using Car_Project.Models;
using Car_Project.Services.Abstractions;
using Car_Project.ViewModels.Compare;
using Microsoft.AspNetCore.Mvc;

namespace Car_Project.Controllers
{
    public class CompareController : Controller
    {
        private readonly ICompareItemService _compareService;

        public CompareController(ICompareItemService compareService)
        {
            _compareService = compareService;
        }

        private string SessionId => HttpContext.Session.Id;

        // GET /Compare
        public async Task<IActionResult> Index()
        {
            var cars = await _compareService.GetCompareListAsync(SessionId);

            var vm = new CompareIndexViewModel
            {
                Cars = cars.Select(MapToViewModel).ToList(),
                AllFeatures = cars
                    .SelectMany(c => c.Features.Select(f => f.CarFeature.Name))
                    .Distinct()
                    .OrderBy(f => f)
                    .ToList()
            };

            return View(vm);
        }

        // POST /Compare/Add/5
        [HttpPost]
        public async Task<IActionResult> Add(int id)
        {
            try
            {
                await _compareService.AddAsync(SessionId, id);
            }
            catch (InvalidOperationException ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = ex.Message });

                TempData["CompareError"] = ex.Message;
                return RedirectBack();
            }

            var count = await _compareService.GetCountAsync(SessionId);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true, count });

            return RedirectBack();
        }

        // POST /Compare/Remove/5
        [HttpPost]
        public async Task<IActionResult> Remove(int id)
        {
            await _compareService.RemoveAsync(SessionId, id);
            var count = await _compareService.GetCountAsync(SessionId);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true, count });

            return RedirectToAction("Index");
        }

        // POST /Compare/Clear
        [HttpPost]
        public async Task<IActionResult> Clear()
        {
            await _compareService.ClearAsync(SessionId);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true, count = 0 });

            return RedirectToAction("Index");
        }

        // GET /Compare/Count
        [HttpGet]
        public async Task<IActionResult> Count()
        {
            var count = await _compareService.GetCountAsync(SessionId);
            return Json(new { count });
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private IActionResult RedirectBack()
        {
            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer))
                return Redirect(referer);
            return RedirectToAction("Index");
        }

        private static CompareCarViewModel MapToViewModel(Car car)
        {
            var mainImage = car.Images.FirstOrDefault(i => i.IsMain)?.ImageUrl
                         ?? car.Images.FirstOrDefault()?.ImageUrl
                         ?? car.ThumbnailUrl;

            return new CompareCarViewModel
            {
                Id           = car.Id,
                Title        = car.Title,
                Price        = car.Price,
                Year         = car.Year,
                Mileage      = car.Mileage,
                FuelType     = car.FuelType.ToString(),
                Transmission = car.Transmission.ToString(),
                BodyStyle    = car.BodyStyle,
                DriveType    = car.DriveType,
                Color        = car.Color,
                Cylinders    = car.Cylinders,
                DoorCount    = car.DoorCount,
                ThumbnailUrl = mainImage,
                BrandName    = car.Brand?.Name ?? string.Empty,
                Condition    = car.Condition.ToString(),
                Features     = car.Features.Select(f => f.CarFeature.Name).ToList()
            };
        }
    }
}
