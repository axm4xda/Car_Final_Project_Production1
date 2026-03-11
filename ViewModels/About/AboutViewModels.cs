using Car_Project.Models;

namespace Car_Project.ViewModels.About
{
    // Komanda üzvü üçün ViewModel
    public class TeamMemberViewModel
    {
        public int SalesAgentId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? FacebookUrl { get; set; }
        public string? TwitterUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? SkypeUrl { get; set; }
        public string? TelegramUrl { get; set; }
    }

    // Statistika elementi üçün ViewModel
    public class StatCounterViewModel
    {
        public string Label { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string? Suffix { get; set; }
    }

    // Mü?t?ri r?yi üçün ViewModel
    public class AboutReviewViewModel
    {
        public string AuthorName { get; set; } = string.Empty;
        public string? AuthorTitle { get; set; }
        public string? AvatarUrl { get; set; }
        public string Content { get; set; } = string.Empty;
        public int Rating { get; set; }
    }

    // Brend üçün ViewModel
    public class AboutBrandViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
    }

    // ?sas About s?hif?si ViewModel
    public class AboutIndexViewModel
    {
        public IList<AboutReviewViewModel> Reviews { get; set; } = new List<AboutReviewViewModel>();
        public IList<TeamMemberViewModel> TeamMembers { get; set; } = new List<TeamMemberViewModel>();
        public IList<StatCounterViewModel> Stats { get; set; } = new List<StatCounterViewModel>();
        public IList<AboutBrandViewModel> Brands { get; set; } = new List<AboutBrandViewModel>();
    }
}
