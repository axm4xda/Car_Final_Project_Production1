using System.ComponentModel.DataAnnotations;

namespace Car_Project.ViewModels.AddListing
{
    public class VipPaymentViewModel
    {
        /// <summary>The pending car id that should become VIP after payment.</summary>
        public int CarId { get; set; }

        /// <summary>Car title – shown on the payment summary.</summary>
        public string CarTitle { get; set; } = string.Empty;

        /// <summary>VIP listing fee (fixed price).</summary>
        public decimal VipFee { get; set; } = 9.99m;

        // ── Payment card details ─────────────────────────────────────
        [Required(ErrorMessage = "Kart üzərindəki ad tələb olunur.")]
        [Display(Name = "Kart Sahibinin Adı")]
        public string CardHolderName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kart nömrəsi tələb olunur.")]
        [RegularExpression(@"^\d{16}$", ErrorMessage = "Kart nömrəsi 16 rəqəmdən ibarət olmalıdır.")]
        [Display(Name = "Kart Nömrəsi")]
        public string CardNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Bitmə tarixi tələb olunur.")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/\d{2}$", ErrorMessage = "Format: MM/YY")]
        [Display(Name = "Bitmə Tarixi")]
        public string ExpiryDate { get; set; } = string.Empty;

        [Required(ErrorMessage = "CVV tələb olunur.")]
        [RegularExpression(@"^\d{3,4}$", ErrorMessage = "CVV 3 və ya 4 rəqəm olmalıdır.")]
        [Display(Name = "CVV")]
        public string Cvv { get; set; } = string.Empty;
    }
}
