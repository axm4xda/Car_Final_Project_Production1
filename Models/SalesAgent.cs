namespace Car_Project.Models
{
    public class SalesAgent : BaseEntity
    {
        public string FullName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? Bio { get; set; }
        public string? Address { get; set; }
        public string? Phone1 { get; set; }
        public string? Phone2 { get; set; }
        public string? Email { get; set; }
        public string? MapEmbedUrl { get; set; }
        public bool IsVerified { get; set; } = false;
        public bool IsActive { get; set; } = true;

        // Social Media
        public string? FacebookUrl { get; set; }
        public string? TwitterUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? SkypeUrl { get; set; }
        public string? TelegramUrl { get; set; }

        // Navigation
        public ICollection<SalesAgentReview> Reviews { get; set; } = new List<SalesAgentReview>();
    }
}
