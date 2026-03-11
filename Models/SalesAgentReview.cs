namespace Car_Project.Models
{
    public class SalesAgentReview : BaseEntity
    {
        public int SalesAgentId { get; set; }
        public SalesAgent SalesAgent { get; set; } = null!;

        public string AuthorName { get; set; } = string.Empty;
        public string? AuthorAvatarUrl { get; set; }
        public string Content { get; set; } = string.Empty;
        public int Rating { get; set; } = 5;
        public bool IsApproved { get; set; } = false;
    }
}
