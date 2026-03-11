using System.ComponentModel.DataAnnotations;

namespace Car_Project.ViewModels.Account
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Ad Soyad mütləqdir")]
        [StringLength(100, ErrorMessage = "Ad Soyad maksimum 100 simvol ola bilər")]
        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email mütləqdir")]
        [EmailAddress(ErrorMessage = "Düzgün email formatı daxil edin")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifrə mütləqdir")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Şifrə minimum 8 simvol olmalıdır")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifrə")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifrəni təkrarlamaq mütləqdir")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Şifrələr uyğun gəlmir")]
        [Display(Name = "Şifrəni Təkrarla")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şərtləri qəbul etməlisiniz")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "İstifadəçi Şərtlərini qəbul etməlisiniz")]
        public bool AgreeToTerms { get; set; }

        [Required(ErrorMessage = "Rol seçimi mütləqdir")]
        [Display(Name = "Rol")]
        public string Role { get; set; } = "User";
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email mütləqdir")]
        [EmailAddress(ErrorMessage = "Düzgün email formatı daxil edin")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifrə mütləqdir")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifrə")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Məni Xatırla")]
        public bool RememberMe { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Email mütləqdir")]
        [EmailAddress(ErrorMessage = "Düzgün email formatı daxil edin")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
    }

    public class UserWithRoleViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        public DateTime CreatedDate { get; set; }
    }
}
