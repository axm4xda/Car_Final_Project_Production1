using Microsoft.AspNetCore.Mvc;

namespace Car_Project.Controllers
{
    public class ComingSoonController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
