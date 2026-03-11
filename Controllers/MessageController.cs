using Car_Project.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Car_Project.Models;

namespace Car_Project.Controllers
{
    [Authorize]
    public class MessageController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<AppUser> _userManager;

        public MessageController(ApplicationDbContext db, UserManager<AppUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public IActionResult Index() => View();

        /// <summary>
        /// Returns the total number of unread messages for the current user.
        /// Called by the layout navbar badge via AJAX.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> UnreadCount()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Json(new { count = 0 });

            var count = await _db.ChatMessages
                .CountAsync(m => m.ReceiverId == userId && !m.IsRead);

            return Json(new { count });
        }
    }
}
