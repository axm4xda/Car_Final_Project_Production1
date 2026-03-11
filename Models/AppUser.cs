using Microsoft.AspNetCore.Identity;

namespace Car_Project.Models
{
    public class AppUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
