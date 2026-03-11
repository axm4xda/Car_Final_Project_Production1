using Microsoft.AspNetCore.Mvc;

namespace Car_Project.Controllers
{
    public class SellYourCarController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
