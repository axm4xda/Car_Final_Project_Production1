using System.ComponentModel.DataAnnotations;
using Car_Project.Models;

namespace Car_Project.ViewModels.Checkout
{
    public class CheckoutFormViewModel
    {
        // ?? Billing ???????????????????????????????????????????????????????????
        [Required(ErrorMessage = "Ad t?l?b olunur")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad t?l?b olunur")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-poçt t?l?b olunur")]
        [EmailAddress(ErrorMessage = "Düzgün e-poçt ünvan? daxil edin")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefon t?l?b olunur")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ölk? t?l?b olunur")]
        public string Country { get; set; } = string.Empty;

        [Required(ErrorMessage = "??h?r t?l?b olunur")]
        public string City { get; set; } = string.Empty;

        public string? Street { get; set; }
        public string? State { get; set; }

        [Required(ErrorMessage = "Poçt kodu t?l?b olunur")]
        public string PostalCode { get; set; } = string.Empty;

        public string? Note { get; set; }

        // ?? Öd?ni? ???????????????????????????????????????????????????????????
        [Required]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CreditCard;

        // Kart m?lumatlar? (yaln?z CreditCard / ApplePay / PayPal üçün)
        public string? CardHolderName { get; set; }
        public string? CardNumber { get; set; }
        public string? CardExpiry { get; set; }
        public string? CardCvv { get; set; }

        // ?? Kupon ????????????????????????????????????????????????????????????
        public string? CouponCode { get; set; }
    }

    public class CheckoutOrderSummaryViewModel
    {
        public IList<CheckoutOrderItemViewModel> Items { get; set; } = new List<CheckoutOrderItemViewModel>();
        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal Total { get; set; }
        public string? AppliedCoupon { get; set; }
    }

    public class CheckoutOrderItemViewModel
    {
        public string ProductName { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => UnitPrice * Quantity;
    }

    public class CheckoutPageViewModel
    {
        public CheckoutFormViewModel Form { get; set; } = new();
        public CheckoutOrderSummaryViewModel Summary { get; set; } = new();
    }
}
