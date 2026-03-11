namespace Car_Project.ViewModels
{
    public class TermsSectionViewModel
    {
        public string AnchorId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public IList<string> Paragraphs { get; set; } = new List<string>();
        public IList<string> BulletPoints { get; set; } = new List<string>();
    }

    public class TermsOfUseViewModel
    {
        public IList<TermsSectionViewModel> Sections { get; set; } = new List<TermsSectionViewModel>();
    }
}
