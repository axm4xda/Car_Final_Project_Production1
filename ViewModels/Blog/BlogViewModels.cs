using Car_Project.Models;
using System.ComponentModel.DataAnnotations;

namespace Car_Project.ViewModels.Blog
{
    public class BlogPostCardViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? AuthorName { get; set; }
        public string? AuthorAvatarUrl { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public DateTime? PublishedAt { get; set; }
        public int ViewCount { get; set; }
        public IList<string> Tags { get; set; } = new List<string>();
    }

    public class BlogDetailViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public string? AuthorName { get; set; }
        public string? AuthorAvatarUrl { get; set; }

        // Sosial media linkləri
        public string? AuthorFacebookUrl { get; set; }
        public string? AuthorTwitterUrl { get; set; }
        public string? AuthorInstagramUrl { get; set; }
        public string? AuthorLinkedInUrl { get; set; }

        public string CategoryName { get; set; } = string.Empty;
        public DateTime? PublishedAt { get; set; }
        public int ViewCount { get; set; }
        public IList<string> Tags { get; set; } = new List<string>();
        public IList<BlogCommentViewModel> Comments { get; set; } = new List<BlogCommentViewModel>();
        public BlogCommentFormViewModel CommentForm { get; set; } = new();
        public IList<BlogPostCardViewModel> RelatedPosts { get; set; } = new List<BlogPostCardViewModel>();

        // Navigation: previous and next posts
        public BlogPostCardViewModel? PrevPost { get; set; }
        public BlogPostCardViewModel? NextPost { get; set; }

        // User login information
        public bool IsUserLoggedIn { get; set; }
        public string? LoggedInUserName { get; set; }
    }

    public class BlogIndexViewModel
    {
        public IList<BlogPostCardViewModel> Posts { get; set; } = new List<BlogPostCardViewModel>();
        public IList<string> Categories { get; set; } = new List<string>();
        public IList<string> PopularTags { get; set; } = new List<string>();
        public IList<BlogPostCardViewModel> RecentPosts { get; set; } = new List<BlogPostCardViewModel>();

        // Filtrasiya
        public string? SelectedCategory { get; set; }
        public string? SelectedTag { get; set; }
        public string? SearchQuery { get; set; }

        // S?hif?l?m?
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
    }

    public class BlogCommentViewModel
    {
        public int Id { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public IList<BlogCommentViewModel> Replies { get; set; } = new List<BlogCommentViewModel>();
    }

    public class BlogCommentFormViewModel
    {
        public int BlogPostId { get; set; }
        public string? AuthorName { get; set; }
        public string? AuthorEmail { get; set; }
        [Required(ErrorMessage = "Şərh boş ola bilməz.")]
        public string Content { get; set; } = string.Empty;
    }
}
