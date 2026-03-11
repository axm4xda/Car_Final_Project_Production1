using Microsoft.AspNetCore.Mvc;

namespace Car_Project.Controllers
{
    public class ContactController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
