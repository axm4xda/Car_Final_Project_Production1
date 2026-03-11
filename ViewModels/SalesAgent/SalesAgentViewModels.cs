namespace Car_Project.ViewModels.SalesAgent
{
    // Index s?hif?si üçün - agent kart?
    public class SalesAgentCardViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? FacebookUrl { get; set; }
        public string? TwitterUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? SkypeUrl { get; set; }
        public string? TelegramUrl { get; set; }
        public string? Phone1 { get; set; }
        public string? Email { get; set; }
    }

    // Index s?hif?si ana ViewModel
    public class SalesAgentIndexViewModel
    {
        public IList<SalesAgentCardViewModel> Agents { get; set; } = new List<SalesAgentCardViewModel>();
    }

    // Review üçün
    public class SalesAgentReviewViewModel
    {
        public string AuthorName { get; set; } = string.Empty;
        public string? AuthorAvatarUrl { get; set; }
        public string Content { get; set; } = string.Empty;
        public int Rating { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    // Details s?hif?si üçün
    public class SalesAgentDetailsViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? Bio { get; set; }
        public string? Address { get; set; }
        public string? Phone1 { get; set; }
        public string? Phone2 { get; set; }
        public string? Email { get; set; }
        public string? MapEmbedUrl { get; set; }
        public bool IsVerified { get; set; }
        public string? FacebookUrl { get; set; }
        public string? TwitterUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? SkypeUrl { get; set; }
        public string? TelegramUrl { get; set; }

        public IList<SalesAgentReviewViewModel> Reviews { get; set; } = new List<SalesAgentReviewViewModel>();

        // Hesablanm?? reytinq m?lumatlar?
        public double AverageRating => Reviews.Count > 0
            ? Math.Round(Reviews.Average(r => r.Rating), 1)
            : 0;

        public int TotalReviews => Reviews.Count;

        public int GetStarPercent(int star) => TotalReviews == 0 ? 0
            : (int)Math.Round((double)Reviews.Count(r => r.Rating == star) / TotalReviews * 100);
    }
}
