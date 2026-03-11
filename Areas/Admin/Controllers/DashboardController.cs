using Car_Project.Data;
using Car_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Car_Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<AppUser> _userManager;

        public DashboardController(ApplicationDbContext db, UserManager<AppUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["ActivePage"] = "Dashboard";
            ViewBag.CarCount = await _db.Cars.CountAsync();
            ViewBag.BrandCount = await _db.Brands.CountAsync();
            ViewBag.ProductCount = await _db.Products.CountAsync();
            ViewBag.OrderCount = await _db.Orders.CountAsync();
            ViewBag.BlogPostCount = await _db.BlogPosts.CountAsync();
            ViewBag.UserCount = await _userManager.Users.CountAsync();
            ViewBag.ContactCount = await _db.ContactMessages.Where(c => !c.IsRead).CountAsync();
            ViewBag.SellRequestCount = await _db.SellCarRequests.Where(s => !s.IsReviewed).CountAsync();
            ViewBag.ReviewCount = await _db.Reviews.CountAsync();
            ViewBag.AgentCount = await _db.SalesAgents.CountAsync();
            ViewBag.SubscriberCount = await _db.NewsletterSubscribers.CountAsync();
            ViewBag.FAQCount = await _db.FAQs.CountAsync();
            ViewBag.RecentOrders = await _db.Orders.OrderByDescending(o => o.CreatedDate).Take(5).ToListAsync();
            ViewBag.RecentMessages = await _db.ContactMessages.OrderByDescending(c => c.CreatedDate).Take(5).ToListAsync();
            return View();
        }
    }
}
