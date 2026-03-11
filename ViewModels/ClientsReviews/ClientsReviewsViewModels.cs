namespace Car_Project.ViewModels.ClientsReviews
{
    // Mü?t?ri r?yi kart? üçün ViewModel
    public class ReviewViewModel
    {
        public int Id { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string? AuthorTitle { get; set; }
        public string? AvatarUrl { get; set; }
        public string Content { get; set; } = string.Empty;
        public int Rating { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    // ?sas ClientsReviews s?hif?si ViewModel
    public class ClientsReviewsIndexViewModel
    {
        public IList<ReviewViewModel> Reviews { get; set; } = new List<ReviewViewModel>();

        // S?hif?l?m?
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }

        // Ortalama reytinq
        public double AverageRating { get; set; }
    }
}
