using Car_Project.Data;
using Car_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Car_Project.Controllers
{
    [Authorize]
    public class UserListingController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _db;

        public UserListingController(UserManager<AppUser> userManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        public async Task<IActionResult> Index(string? search, string? sort, int page = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Index", "Home");

            int pageSize = 10;

            var query = _db.Cars
                .Include(c => c.Brand)
                .Include(c => c.Images)
                .Where(c => c.UserId == user.Id)   // ← yalnız öz elanları
                .AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c =>
                    c.Title.Contains(search) ||
                    c.Brand.Name.Contains(search));
            }

            // Sort
            query = sort?.ToLower() switch
            {
                "lowest-price" => query.OrderBy(c => c.Price),
                "highest-price" => query.OrderByDescending(c => c.Price),
                "lowest-mileage" => query.OrderBy(c => c.Mileage),
                "highest-mileage" => query.OrderByDescending(c => c.Mileage),
                _ => query.OrderByDescending(c => c.CreatedDate)
            };

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var cars = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Cars = cars;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.SearchQuery = search;
            ViewBag.SelectedSort = sort ?? "newest";

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Index", "Home");

            var car = await _db.Cars.FirstOrDefaultAsync(c => c.Id == id && c.UserId == user.Id);
            if (car == null)
            {
                TempData["AuthError"] = "Bu elanı silmək icazəniz yoxdur!";
                return RedirectToAction("Index");
            }

            _db.Cars.Remove(car);
            await _db.SaveChangesAsync();
            TempData["AuthSuccess"] = "Elan uğurla silindi!";

            return RedirectToAction("Index");
        }
    }
}
