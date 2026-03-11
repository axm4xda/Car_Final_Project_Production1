using Car_Project.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Car_Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // GET: Admin/Notification
        public async Task<IActionResult> Index()
        {
            ViewData["ActivePage"] = "Notifications";
            ViewData["Title"] = "Bildirişlər";
            var notifications = await _notificationService.GetAdminNotificationsAsync();
            return View(notifications);
        }

        // POST: Admin/Notification/MarkAsRead/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notification = await _notificationService.GetByIdAsync(id);
            if (notification == null) return NotFound();

            await _notificationService.MarkAsReadAsync(id);

            if (!string.IsNullOrEmpty(notification.Link))
                return Redirect(notification.Link);

            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Notification/MarkAllAsRead
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllAsRead()
        {
            await _notificationService.MarkAllAdminAsReadAsync();
            TempData["Success"] = "Bütün bildirişlər oxunmuş olaraq işarələndi.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Notification/Delete/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _notificationService.DeleteAsync(id);
            TempData["Success"] = "Bildiriş silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
