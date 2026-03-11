using System.ComponentModel.DataAnnotations;

namespace Car_Project.ViewModels.Calculator
{
    // Kredit kalkulyator formas? üçün ViewModel
    public class LoanCalculatorFormViewModel
    {
        [Required(ErrorMessage = "Avtomobil qiym?ti t?l?b olunur")]
        [Range(1, double.MaxValue, ErrorMessage = "Düzgün qiym?t daxil edin")]
        [Display(Name = "Car Price")]
        public decimal CarPrice { get; set; } = 46300;

        [Range(0, double.MaxValue)]
        [Display(Name = "Down Payment")]
        public decimal DownPayment { get; set; } = 400;

        [Range(0, double.MaxValue)]
        [Display(Name = "Trade In Value")]
        public decimal TradeInValue { get; set; } = 0;

        [Required(ErrorMessage = "Faiz d?r?c?si t?l?b olunur")]
        [Range(0.1, 100, ErrorMessage = "Düzgün faiz d?r?c?si daxil edin")]
        [Display(Name = "Interest Rate (%)")]
        public decimal InterestRate { get; set; } = 1.20m;

        [Range(0, 100)]
        [Display(Name = "Sales Tax (%)")]
        public decimal SalesTax { get; set; } = 3.00m;

        [Required(ErrorMessage = "Kredit müdd?ti t?l?b olunur")]
        [Display(Name = "Loan Term (months)")]
        public int LoanTermMonths { get; set; } = 36;

        public IList<int> AvailableTerms { get; set; } = new List<int> { 12, 24, 36, 48, 60, 72, 84 };
    }

    // Kalkulyator n?tic?si üçün ViewModel
    public class LoanCalculatorResultViewModel
    {
        public decimal MonthlyPayment { get; set; }
        public decimal TotalInterestPayment { get; set; }
        public decimal TotalLoanAmount { get; set; }
        public decimal CarPrice { get; set; }
        public decimal DownPayment { get; set; }
        public decimal TradeInValue { get; set; }
        public decimal InterestRate { get; set; }
        public decimal SalesTax { get; set; }
        public decimal SalesTaxAmount { get; set; }
        public int LoanTermMonths { get; set; }
        public int LoanTermYears => LoanTermMonths / 12;
        public int LoanTermRemainderMonths => LoanTermMonths % 12;
    }

    // ?sas Calculator s?hif?si ViewModel
    public class CalculatorIndexViewModel
    {
        public LoanCalculatorFormViewModel Form { get; set; } = new();
        public LoanCalculatorResultViewModel? Result { get; set; }
        public bool HasResult => Result != null;
    }
}
