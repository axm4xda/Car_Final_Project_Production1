using System.ComponentModel.DataAnnotations;

namespace Car_Project.ViewModels.Contact
{
    // ?laq? formas? üçün ViewModel
    public class ContactFormViewModel
    {
        [Required(ErrorMessage = "Ad t?l?b olunur")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-poçt t?l?b olunur")]
        [EmailAddress(ErrorMessage = "Düzgün e-poçt ünvan? daxil edin")]
        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }

        [Required(ErrorMessage = "Mövzu t?l?b olunur")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mesaj t?l?b olunur")]
        public string Message { get; set; } = string.Empty;
    }

    // ?laq? m?lumatlar? üçün ViewModel
    public class ContactInfoViewModel
    {
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? WorkingHours { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    // ?sas Contact s?hif?si ViewModel
    public class ContactIndexViewModel
    {
        public ContactFormViewModel Form { get; set; } = new();
        public ContactInfoViewModel Info { get; set; } = new();
        public bool IsSuccess { get; set; }
    }
}
