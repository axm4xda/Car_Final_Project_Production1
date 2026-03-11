namespace Car_Project.ViewModels
{
    public class FAQItemViewModel
    {
        public string Question { get; set; } = string.Empty;
        public string Answer   { get; set; } = string.Empty;
        public bool   IsOpen   { get; set; }
    }

    public class FAQGroupViewModel
    {
        public string GroupTitle { get; set; } = string.Empty;
        public IList<FAQItemViewModel> Items { get; set; } = new List<FAQItemViewModel>();
    }
}
