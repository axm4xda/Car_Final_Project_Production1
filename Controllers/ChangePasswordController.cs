using Car_Project.Models;
using Car_Project.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Car_Project.Controllers
{
    [Authorize]
    public class ChangePasswordController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailService _emailService;
        private readonly ILogger<ChangePasswordController> _logger;

        public ChangePasswordController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IEmailService emailService,
            ILogger<ChangePasswordController> logger)
        {
            _userManager   = userManager;
            _signInManager = signInManager;
            _emailService  = emailService;
            _logger        = logger;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Index", "Home");

            ViewBag.Email = user.Email;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string oldPassword, string newPassword, string retypeNewPassword)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Index", "Home");

            ViewBag.Email = user.Email;

            if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword))
            {
                ViewBag.Error = "Bütün sahələri doldurun.";
                return View();
            }

            if (newPassword != retypeNewPassword)
            {
                ViewBag.Error = "Yeni şifrələr uyğun gəlmir.";
                return View();
            }

            var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);

                // ── Şifrə dəyişikliyi təhlükəsizlik maili göndər ─────────────
                if (!string.IsNullOrWhiteSpace(user.Email))
                {
                    try
                    {
                        await _emailService.SendPasswordChangedAsync(user.Email, user.FullName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Password changed email failed for {Email}", user.Email);
                        /* email failure should not block password change */
                    }
                }

                ViewBag.Success = "Şifrə uğurla dəyişdirildi!";
                return View();
            }

            ViewBag.Error = string.Join("<br>", result.Errors.Select(e => e.Description));
            return View();
        }
    }
}
