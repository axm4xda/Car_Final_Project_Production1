using Car_Project.Data;
using Car_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Car_Project.Controllers
{
    [Authorize]
    public class UserProfilController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ApplicationDbContext _db;

        public UserProfilController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            ApplicationDbContext db)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Index", "Home");

            var roles = await _userManager.GetRolesAsync(user);

            ViewBag.User = user;
            ViewBag.Role = roles.FirstOrDefault() ?? "User";
            ViewBag.CarCount = await _db.Cars.CountAsync(c => c.UserId == user.Id);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(string fullName, string? phoneNumber)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Index", "Home");

            if (!string.IsNullOrWhiteSpace(fullName))
            {
                user.FullName = fullName.Trim();
            }

            user.PhoneNumber = phoneNumber?.Trim();

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["AuthSuccess"] = "Profil uğurla yeniləndi!";
            }
            else
            {
                TempData["AuthError"] = string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction("Index");
        }
    }
}
