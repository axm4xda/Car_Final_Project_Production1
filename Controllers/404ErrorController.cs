using Microsoft.AspNetCore.Mvc;

namespace Car_Project.Controllers
{
    [Route("404Error")]
    public class ErrorController : Controller
    {
        [Route("{statusCode?}")]
        [Route("")]
        public IActionResult Index(int? statusCode)
        {
            return View("~/Views/404Error/Index.cshtml");
        }
    }
}
