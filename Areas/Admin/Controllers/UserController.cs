using Car_Project.Models;
using Car_Project.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Car_Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperAdmin")]
    public class UserController : Controller
    {
        private readonly UserManager<AppUser> _userManager;

        public UserController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        // GET: /Admin/User
        public async Task<IActionResult> Index()
        {
            ViewData["ActivePage"] = "Users";

            var users = await _userManager.Users
                .OrderByDescending(u => u.CreatedDate)
                .ToListAsync();

            var userList = new List<UserWithRoleViewModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userList.Add(new UserWithRoleViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email ?? "",
                    Role = roles.FirstOrDefault() ?? "User",
                    CreatedDate = user.CreatedDate
                });
            }

            return View(userList);
        }

        // GET: /Admin/User/Details/{id}
        public async Task<IActionResult> Details(string id)
        {
            ViewData["ActivePage"] = "Users";

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.UserRole = roles.FirstOrDefault() ?? "User";

            return View(user);
        }

        // POST: /Admin/User/ChangeRole
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(string userId, string newRole)
        {
            var allowedRoles = new[] { "Admin", "Agent", "User" };
            if (!allowedRoles.Contains(newRole))
            {
                TempData["Error"] = "Yanlış rol seçimi.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "İstifadəçi tapılmadı.";
                return RedirectToAction(nameof(Index));
            }

            if (await _userManager.IsInRoleAsync(user, "SuperAdmin"))
            {
                TempData["Error"] = "SuperAdmin-in rolunu dəyişmək mümkün deyil.";
                return RedirectToAction(nameof(Index));
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, newRole);

            TempData["Success"] = $"{user.FullName} istifadəçisinə \"{newRole}\" rolu təyin edildi.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/User/Delete
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "İstifadəçi tapılmadı.";
                return RedirectToAction(nameof(Index));
            }

            if (await _userManager.IsInRoleAsync(user, "SuperAdmin"))
            {
                TempData["Error"] = "SuperAdmin hesabını silmək mümkün deyil.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = $"{user.FullName} istifadəçisi silindi.";
            }
            else
            {
                TempData["Error"] = "İstifadəçini silmək mümkün olmadı.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
