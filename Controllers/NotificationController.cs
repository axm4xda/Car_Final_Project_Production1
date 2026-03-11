using Car_Project.Models;
using Car_Project.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Car_Project.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly UserManager<AppUser> _userManager;

        public NotificationController(INotificationService notificationService, UserManager<AppUser> userManager)
        {
            _notificationService = notificationService;
            _userManager = userManager;
        }

        // GET: /Notification
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Index", "Home");

            var notifications = await _notificationService.GetByUserIdAsync(user.Id);
            return View(notifications);
        }

        // POST: /Notification/MarkAsRead/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var notification = await _notificationService.GetByIdAsync(id);
            if (notification == null || notification.UserId != user.Id)
                return NotFound();

            await _notificationService.MarkAsReadAsync(id);

            if (!string.IsNullOrEmpty(notification.Link))
                return Redirect(notification.Link);

            return RedirectToAction(nameof(Index));
        }

        // POST: /Notification/MarkAllAsRead
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            await _notificationService.MarkAllAsReadAsync(user.Id);
            return RedirectToAction(nameof(Index));
        }

        // POST: /Notification/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var notification = await _notificationService.GetByIdAsync(id);
            if (notification == null || notification.UserId != user.Id)
                return NotFound();

            await _notificationService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // GET: /Notification/GetUnreadCount (AJAX)
        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { count = 0 });

            var count = await _notificationService.GetUnreadCountAsync(user.Id);
            return Json(new { count });
        }

        // GET: /Notification/GetLatest (AJAX — dropdown üçün)
        [HttpGet]
        public async Task<IActionResult> GetLatest()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { notifications = Array.Empty<object>() });

            var all = await _notificationService.GetByUserIdAsync(user.Id);
            var latest = all.Take(5).Select(n => new
            {
                n.Id,
                n.Title,
                n.Message,
                type = n.Type.ToString(),
                n.IsRead,
                n.Link,
                timeAgo = GetTimeAgo(n.CreatedDate)
            });

            return Json(new { notifications = latest });
        }

        private static string GetTimeAgo(DateTime createdDate)
        {
            var diff = DateTime.UtcNow - createdDate;
            if (diff.TotalMinutes < 1) return "İndicə";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} dəq əvvəl";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} saat əvvəl";
            if (diff.TotalDays < 30) return $"{(int)diff.TotalDays} gün əvvəl";
            return createdDate.ToString("dd.MM.yyyy");
        }
    }
}
