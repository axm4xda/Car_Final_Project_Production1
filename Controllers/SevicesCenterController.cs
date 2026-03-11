using Car_Project.Services.Abstractions;
using Car_Project.ViewModels.ServicesCenter;
using Microsoft.AspNetCore.Mvc;

namespace Car_Project.Controllers
{
    public class SevicesCenterController : Controller
    {
        private readonly IServiceCenterService _serviceCenterService;

        public SevicesCenterController(IServiceCenterService serviceCenterService)
        {
            _serviceCenterService = serviceCenterService;
        }

        public async Task<IActionResult> Index(string? q = null)
        {
            var all = await _serviceCenterService.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(q))
                all = all.Where(s =>
                    s.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    (s.Address != null && s.Address.Contains(q, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

            var vm = new ServicesCenterIndexViewModel
            {
                ServiceCenters = all.Select(s => new ServiceCenterCardViewModel
                {
                    Id           = s.Id,
                    Name         = s.Name,
                    Address      = s.Address ?? "",
                    Phone        = s.Phone,
                    Email        = s.Email,
                    ImageUrl     = s.ImageUrl,
                    WorkingHours = s.WorkingHours,
                    Latitude     = s.Latitude,
                    Longitude    = s.Longitude
                }).ToList(),
                SearchQuery = q,
                TotalCount  = all.Count
            };

            return View(vm);
        }
    }
}
