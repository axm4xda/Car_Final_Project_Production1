namespace Car_Project.Models
{
    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        CarApproved,
        CarRejected,
        SellRequestApproved,
        SellRequestRejected,
        NewContactMessage,
        NewSellRequest,
        NewCarListing,
        AdminReply,
        NewReview,
        NewBlogComment,
        NewChatMessage,
        ReviewReply,
        BlogCommentReply,
        CouponIssued,
        NewOrder
    }

    public class Notification : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; } = NotificationType.Info;
        public bool IsRead { get; set; }

        /// <summary>Bildirişin ünvan edildiyi istifadəçi (null = bütün adminlər üçün)</summary>
        public string? UserId { get; set; }
        public AppUser? User { get; set; }

        /// <summary>Bildirişlə əlaqəli keçid linki</summary>
        public string? Link { get; set; }
    }
}
