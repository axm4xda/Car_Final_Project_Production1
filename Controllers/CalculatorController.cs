using Car_Project.ViewModels.Calculator;
using Microsoft.AspNetCore.Mvc;

namespace Car_Project.Controllers
{
    public class CalculatorController : Controller
    {
        [HttpGet]
        public IActionResult Index(
            decimal? carPrice     = null,
            decimal? downPayment  = null,
            int?     loanTerm     = null,
            decimal? interestRate = null)
        {
            var form = new LoanCalculatorFormViewModel();

            if (carPrice.HasValue)     form.CarPrice        = carPrice.Value;
            if (downPayment.HasValue)  form.DownPayment     = downPayment.Value;
            if (loanTerm.HasValue && form.AvailableTerms.Contains(loanTerm.Value))
                                       form.LoanTermMonths  = loanTerm.Value;
            if (interestRate.HasValue) form.InterestRate    = interestRate.Value;

            var vm = new CalculatorIndexViewModel { Form = form };

            // Auto-calculate when pre-filled from car detail page
            if (carPrice.HasValue)
                vm.Result = Calculate(form);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(LoanCalculatorFormViewModel form)
        {
            var vm = new CalculatorIndexViewModel { Form = form };

            if (!ModelState.IsValid)
                return View(vm);

            vm.Result = Calculate(form);
            return View(vm);
        }

        private static LoanCalculatorResultViewModel Calculate(LoanCalculatorFormViewModel f)
        {
            // Sales tax məbləği
            decimal salesTaxAmount = Math.Round(f.CarPrice * (f.SalesTax / 100m), 2);

            // Principal = CarPrice - DownPayment - TradeInValue + SalesTaxAmount
            decimal principal = f.CarPrice - f.DownPayment - f.TradeInValue + salesTaxAmount;
            if (principal < 0) principal = 0;

            decimal monthlyPayment;
            decimal totalInterest;

            if (f.InterestRate == 0m || f.LoanTermMonths == 0)
            {
                // Faizsiz kredit — sadə bölmə
                monthlyPayment = f.LoanTermMonths > 0
                    ? Math.Round(principal / f.LoanTermMonths, 2)
                    : 0m;
                totalInterest = 0m;
            }
            else
            {
                // Annuitet formulu tam decimal ilə:
                // r = aylıq faiz dərəcəsi
                // M = P * r * (1+r)^n / ((1+r)^n - 1)
                decimal r = f.InterestRate / 100m / 12m;
                int n = f.LoanTermMonths;

                // (1 + r)^n — decimal ilə dəqiq hesab
                decimal factor = DecimalPow(1m + r, n);

                monthlyPayment = Math.Round(principal * r * factor / (factor - 1m), 2);
                totalInterest  = Math.Round(monthlyPayment * n - principal, 2);
                if (totalInterest < 0m) totalInterest = 0m;
            }

            decimal totalLoanAmount = Math.Round(principal + totalInterest, 2);

            return new LoanCalculatorResultViewModel
            {
                CarPrice             = f.CarPrice,
                DownPayment          = f.DownPayment,
                TradeInValue         = f.TradeInValue,
                InterestRate         = f.InterestRate,
                SalesTax             = f.SalesTax,
                SalesTaxAmount       = salesTaxAmount,
                LoanTermMonths       = f.LoanTermMonths,
                MonthlyPayment       = monthlyPayment,
                TotalInterestPayment = totalInterest,
                TotalLoanAmount      = totalLoanAmount
            };
        }

        /// <summary>
        /// decimal üçün dəqiq qüvvət hesablaması (double.Math.Pow əvəzinə).
        /// </summary>
        private static decimal DecimalPow(decimal baseVal, int exp)
        {
            decimal result = 1m;
            for (int i = 0; i < exp; i++)
                result *= baseVal;
            return result;
        }
    }
}
