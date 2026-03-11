using Car_Project.Data;
using Car_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Car_Project.Controllers
{
    [Authorize]
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<AppUser> _userManager;

        public ReviewsController(ApplicationDbContext db, UserManager<AppUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int? rating, string? sort, string? search, int page = 1)
        {
            int pageSize = 9;

            var query = _db.Reviews.AsQueryable();

            // Filter by rating
            if (rating.HasValue && rating.Value >= 1 && rating.Value <= 5)
            {
                query = query.Where(r => r.Rating == rating.Value);
            }

            // Search by keyword
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(r =>
                    r.AuthorName.Contains(search) ||
                    r.Content.Contains(search) ||
                    (r.AuthorTitle != null && r.AuthorTitle.Contains(search)));
            }

            // Sort
            query = sort?.ToLower() switch
            {
                "asc" => query.OrderBy(r => r.CreatedDate),
                _ => query.OrderByDescending(r => r.CreatedDate)
            };

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var reviews = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Reviews = reviews;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.SelectedRating = rating;
            ViewBag.SelectedSort = sort ?? "desc";
            ViewBag.SearchQuery = search;

            return View();
        }
    }
}
