using Car_Project.Services.Abstractions;
using Car_Project.ViewModels.ClientsReviews;
using Microsoft.AspNetCore.Mvc;

namespace Car_Project.Controllers
{
    public class ClientsReviewsController : Controller
    {
        private readonly IReviewService _reviewService;

        public ClientsReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            const int pageSize = 9;
            var allReviews = await _reviewService.GetApprovedAsync();

            var totalCount = allReviews.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            if (page < 1) page = 1;

            var paged = allReviews
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new ReviewViewModel
                {
                    Id = r.Id,
                    AuthorName = r.AuthorName,
                    AuthorTitle = r.AuthorTitle,
                    AvatarUrl = r.AvatarUrl,
                    Content = r.Content,
                    Rating = r.Rating,
                    CreatedDate = r.CreatedDate
                }).ToList();

            var vm = new ClientsReviewsIndexViewModel
            {
                Reviews = paged,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalCount = totalCount,
                AverageRating = totalCount > 0 ? allReviews.Average(r => r.Rating) : 0
            };

            return View(vm);
        }
    }
}
