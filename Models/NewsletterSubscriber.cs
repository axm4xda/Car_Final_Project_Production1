namespace Car_Project.Models
{
    public class NewsletterSubscriber : BaseEntity
    {
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
