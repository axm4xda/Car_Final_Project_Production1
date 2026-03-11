namespace Car_Project.Models
{
    public class Review : BaseEntity
    {
        public string AuthorName { get; set; } = string.Empty;
        public string? AuthorTitle { get; set; }
        public string? AvatarUrl { get; set; }
        public string Content { get; set; } = string.Empty;

        /// <summary>1–5 arasında reytinq</summary>
        public int Rating { get; set; } = 5;

        public bool IsApproved { get; set; }

        /// <summary>Müəyyən bir avtomobilə aid review-dur (null = ümumi rəy)</summary>
        public int? CarId { get; set; }
        public Car? Car { get; set; }

        /// <summary>Review-u yazan istifadəçi</summary>
        public string? UserId { get; set; }
        public AppUser? User { get; set; }

        /// <summary>Cavab verilən review (null = əsas review)</summary>
        public int? ParentReviewId { get; set; }
        public Review? ParentReview { get; set; }
        public ICollection<Review> Replies { get; set; } = new List<Review>();
    }
}
