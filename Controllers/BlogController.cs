using Car_Project.Data;
using Car_Project.Services.Abstractions;
using Car_Project.ViewModels.Blog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Car_Project.Models;

namespace Car_Project.Controllers
{
    public class BlogController : Controller
    {
        private readonly IBlogService _blogService;
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _context;

        public BlogController(IBlogService blogService, UserManager<AppUser> userManager, ApplicationDbContext context)
        {
            _blogService = blogService;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index(
            string? category = null,
            string? tag = null,
            string? q = null,
            int page = 1)
        {
            const int pageSize = 9;

            var (posts, totalCount) = await _blogService.GetPublishedAsync(
                page, pageSize, category, tag, q);

            var categories  = await _blogService.GetCategoriesAsync();
            var tags        = await _blogService.GetTagsAsync();
            var recentPosts = await _blogService.GetRecentAsync(5);

            var vm = new BlogIndexViewModel
            {
                Posts = posts.Select(p => new BlogPostCardViewModel
                {
                    Id              = p.Id,
                    Title           = p.Title,
                    Slug            = p.Slug,
                    Summary         = p.Summary,
                    ThumbnailUrl    = p.ThumbnailUrl,
                    AuthorName      = p.AuthorName,
                    AuthorAvatarUrl = p.AuthorAvatarUrl,
                    CategoryName    = p.Category?.Name ?? string.Empty,
                    PublishedAt     = p.PublishedAt,
                    ViewCount       = p.ViewCount,
                    Tags            = p.PostTags.Select(pt => pt.BlogTag.Name).ToList()
                }).ToList(),

                Categories    = categories.Select(c => c.Name).ToList(),
                PopularTags   = tags.Select(t => t.Name).ToList(),

                RecentPosts = recentPosts.Select(p => new BlogPostCardViewModel
                {
                    Id           = p.Id,
                    Title        = p.Title,
                    Slug         = p.Slug,
                    ThumbnailUrl = p.ThumbnailUrl,
                    PublishedAt  = p.PublishedAt,
                    CategoryName = p.Category?.Name ?? string.Empty
                }).ToList(),

                SelectedCategory = category,
                SelectedTag      = tag,
                SearchQuery      = q,
                CurrentPage      = page,
                TotalPages       = (int)Math.Ceiling(totalCount / (double)pageSize),
                TotalCount       = totalCount
            };

            return View(vm);
        }

        public async Task<IActionResult> Detail(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return NotFound();

            var post = await _blogService.GetBySlugAsync(slug);
            if (post == null)
                return NotFound();

            await _blogService.IncrementViewCountAsync(post.Id);

            var relatedPosts = await _blogService.GetRelatedAsync(post.CategoryId, post.Id, 3);
            var categories = await _blogService.GetCategoriesAsync();
            var tags = await _blogService.GetTagsAsync();
            var recentPosts = await _blogService.GetRecentAsync(4);
            var (prevPost, nextPost) = await _blogService.GetAdjacentPostsAsync(post.Id);

            // Login olan istifadəçinin məlumatlarını götür
            var currentUser = User.Identity?.IsAuthenticated == true
                ? await _userManager.GetUserAsync(User)
                : null;

            // Load top-level approved comments with their approved replies
            var commentsWithReplies = await _context.BlogComments
                .Where(c => c.BlogPostId == post.Id && c.IsApproved && c.ParentCommentId == null)
                .Include(c => c.Replies.Where(r => r.IsApproved))
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();

            var vm = new BlogDetailViewModel
            {
                Id              = post.Id,
                Title           = post.Title,
                Slug            = post.Slug,
                Content         = post.Content,
                ThumbnailUrl    = post.ThumbnailUrl,
                AuthorName      = post.AuthorName,
                AuthorAvatarUrl = post.AuthorAvatarUrl,
                AuthorFacebookUrl  = post.AuthorFacebookUrl,
                AuthorTwitterUrl   = post.AuthorTwitterUrl,
                AuthorInstagramUrl = post.AuthorInstagramUrl,
                AuthorLinkedInUrl  = post.AuthorLinkedInUrl,
                CategoryName = post.Category?.Name ?? "",
                PublishedAt = post.PublishedAt,
                ViewCount = post.ViewCount,
                Tags = post.PostTags.Select(pt => pt.BlogTag.Name).ToList(),
                Comments = commentsWithReplies.Select(c => new BlogCommentViewModel
                {
                    Id = c.Id,
                    AuthorName = c.AuthorName,
                    Content = c.Content,
                    CreatedDate = c.CreatedDate,
                    Replies = c.Replies
                        .OrderBy(r => r.CreatedDate)
                        .Select(r => new BlogCommentViewModel
                        {
                            Id = r.Id,
                            AuthorName = r.AuthorName,
                            Content = r.Content,
                            CreatedDate = r.CreatedDate
                        }).ToList()
                }).ToList(),
                CommentForm = new BlogCommentFormViewModel { BlogPostId = post.Id },
                IsUserLoggedIn = currentUser != null,
                LoggedInUserName = currentUser?.FullName ?? currentUser?.UserName,
                RelatedPosts = relatedPosts.Select(p => new BlogPostCardViewModel
                {
                    Id = p.Id,
                    Title = p.Title,
                    Slug = p.Slug,
                    ThumbnailUrl = p.ThumbnailUrl,
                    AuthorName = p.AuthorName,
                    PublishedAt = p.PublishedAt,
                    CategoryName = p.Category?.Name ?? ""
                }).ToList(),
                PrevPost = prevPost == null ? null : new BlogPostCardViewModel
                {
                    Id = prevPost.Id,
                    Title = prevPost.Title,
                    Slug = prevPost.Slug,
                    ThumbnailUrl = prevPost.ThumbnailUrl,
                    CategoryName = prevPost.Category?.Name ?? "",
                    PublishedAt = prevPost.PublishedAt
                },
                NextPost = nextPost == null ? null : new BlogPostCardViewModel
                {
                    Id = nextPost.Id,
                    Title = nextPost.Title,
                    Slug = nextPost.Slug,
                    ThumbnailUrl = nextPost.ThumbnailUrl,
                    CategoryName = nextPost.Category?.Name ?? "",
                    PublishedAt = nextPost.PublishedAt
                }
            };

            ViewBag.Categories = categories;
            ViewBag.AllTags = tags;
            ViewBag.RecentPosts = recentPosts;

            return View(vm);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(BlogCommentFormViewModel form)
        {
            // Login olan istifadəçinin məlumatlarını götür
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return RedirectToAction("Index", "Account");

            if (string.IsNullOrWhiteSpace(form.Content))
            {
                var post2 = await _blogService.GetByIdAsync(form.BlogPostId);
                return RedirectToAction(nameof(Detail), new { slug = post2?.Slug });
            }

            var post = await _blogService.GetByIdAsync(form.BlogPostId);
            if (post == null)
                return NotFound();

            await _blogService.AddCommentAsync(new BlogComment
            {
                BlogPostId  = form.BlogPostId,
                AuthorName  = currentUser.FullName ?? currentUser.UserName ?? "User",
                AuthorEmail = currentUser.Email ?? string.Empty,
                Content     = form.Content
            });

            return RedirectToAction(nameof(Detail), new { slug = post.Slug });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReplyToComment(int blogPostId, int parentCommentId, string content)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return RedirectToAction("Index", "Account");

            var post = await _blogService.GetByIdAsync(blogPostId);
            if (post == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(content))
                return RedirectToAction(nameof(Detail), new { slug = post.Slug });

            var parentComment = await _context.BlogComments.FindAsync(parentCommentId);
            if (parentComment == null)
                return NotFound();

            var reply = new BlogComment
            {
                BlogPostId = blogPostId,
                ParentCommentId = parentCommentId,
                AuthorName = currentUser.FullName ?? currentUser.UserName ?? "User",
                AuthorEmail = currentUser.Email ?? string.Empty,
                Content = content,
                IsApproved = true,
                CreatedDate = DateTime.UtcNow
            };

            await _context.BlogComments.AddAsync(reply);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Detail), new { slug = post.Slug });
        }
    }
}
