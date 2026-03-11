namespace Car_Project.Models
{
    public class FAQ : BaseEntity
    {
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public string? GroupName { get; set; }
        public int Order { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
