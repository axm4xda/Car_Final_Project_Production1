using System.ComponentModel.DataAnnotations;

namespace Car_Project.ViewModels.SellYourCar
{
    // Avtomobili satmaq üçün form ViewModel
    public class SellCarFormViewModel
    {
        [Required(ErrorMessage = "Ad t?l?b olunur")]
        [Display(Name = "Full Name")]
        public string OwnerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-poçt t?l?b olunur")]
        [EmailAddress(ErrorMessage = "Düzgün e-poçt ünvan? daxil edin")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefon t?l?b olunur")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Avtomobil ad? t?l?b olunur")]
        [Display(Name = "Car Title")]
        public string CarTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "?l t?l?b olunur")]
        [Range(1990, 2030, ErrorMessage = "Düzgün il daxil edin")]
        public int Year { get; set; }

        [Required(ErrorMessage = "Yürü? t?l?b olunur")]
        [Range(0, int.MaxValue, ErrorMessage = "Düzgün yürü? daxil edin")]
        public int Mileage { get; set; }

        public string? FuelType { get; set; }
        public string? Transmission { get; set; }

        [Required(ErrorMessage = "Qiym?t t?l?b olunur")]
        [Range(0, double.MaxValue, ErrorMessage = "Düzgün qiym?t daxil edin")]
        [Display(Name = "Asking Price")]
        public decimal AskingPrice { get; set; }

        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
    }

    // ?sas SellYourCar s?hif?si ViewModel
    public class SellYourCarIndexViewModel
    {
        public SellCarFormViewModel Form { get; set; } = new();
        public bool IsSuccess { get; set; }
    }
}
