namespace Car_Project.Models
{
    public enum BlogStatus
    {
        Draft,
        Published,
        Archived
    }

    public class BlogCategory : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Slug { get; set; }

        public ICollection<BlogPost> Posts { get; set; } = new List<BlogPost>();
    }

    public class BlogPost : BaseEntity
    {
        public string Title { get; set; } = string.Empty;

        /// <summary>URL-friendly başlıq: "my-first-post"</summary>
        public string Slug { get; set; } = string.Empty;

        public string? Summary { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public string? AuthorName { get; set; }
        public string? AuthorAvatarUrl { get; set; }

        // Sosial media linkləri (optional)
        public string? AuthorFacebookUrl { get; set; }
        public string? AuthorTwitterUrl { get; set; }
        public string? AuthorInstagramUrl { get; set; }
        public string? AuthorLinkedInUrl { get; set; }

        public int ViewCount { get; set; }
        public BlogStatus Status { get; set; } = BlogStatus.Draft;
        public DateTime? PublishedAt { get; set; }

        public int CategoryId { get; set; }
        public BlogCategory Category { get; set; } = null!;

        public ICollection<BlogPostTag> PostTags { get; set; } = new List<BlogPostTag>();
        public ICollection<BlogComment> Comments { get; set; } = new List<BlogComment>();
    }

    public class BlogTag : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public ICollection<BlogPostTag> PostTags { get; set; } = new List<BlogPostTag>();
    }

    public class BlogPostTag
    {
        public int BlogPostId { get; set; }
        public BlogPost BlogPost { get; set; } = null!;

        public int BlogTagId { get; set; }
        public BlogTag BlogTag { get; set; } = null!;
    }

    public class BlogComment : BaseEntity
    {
        public string AuthorName { get; set; } = string.Empty;
        public string AuthorEmail { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsApproved { get; set; }

        public int BlogPostId { get; set; }
        public BlogPost BlogPost { get; set; } = null!;

        /// <summary>Cavab verilən şərh (null = əsas şərh)</summary>
        public int? ParentCommentId { get; set; }
        public BlogComment? ParentComment { get; set; }
        public ICollection<BlogComment> Replies { get; set; } = new List<BlogComment>();
    }
}
