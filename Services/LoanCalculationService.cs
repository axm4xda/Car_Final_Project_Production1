using Car_Project.Data;
using Car_Project.Models;
using Car_Project.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Car_Project.Services
{
    public class LoanCalculationService : ILoanCalculationService
    {
        private readonly ApplicationDbContext _context;

        public LoanCalculationService(ApplicationDbContext context)
        {
            _context = context;
        }

        // PUBLIC

        public LoanCalculation Calculate(
            decimal carPrice,
            decimal downPayment,
            decimal annualInterestRate,
            int loanTermMonths)
        {
            if (carPrice <= 0)
                throw new ArgumentOutOfRangeException(nameof(carPrice), "Avtomobil qiyməti 0-dan böyük olmalıdır.");

            if (downPayment < 0 || downPayment >= carPrice)
                throw new ArgumentOutOfRangeException(nameof(downPayment), "İlkin ödəniş 0 ilə avtomobil qiyməti arasında olmalıdır.");

            if (annualInterestRate < 0)
                throw new ArgumentOutOfRangeException(nameof(annualInterestRate), "Faiz dərəcəsi mənfi ola bilməz.");

            if (loanTermMonths <= 0)
                throw new ArgumentOutOfRangeException(nameof(loanTermMonths), "Kredit müddəti 0-dan böyük olmalıdır.");

            var principal = carPrice - downPayment;

            decimal monthlyPayment;
            decimal totalInterest;

            if (annualInterestRate == 0)
            {
                // Faizsiz kredit
                monthlyPayment = principal / loanTermMonths;
                totalInterest  = 0;
            }
            else
            {
                // Annuitet (bərabər aylıq ödəniş) formulu:
                // M = P * [r(1+r)^n] / [(1+r)^n - 1]
                var r = (double)(annualInterestRate / 100m / 12m);
                var n = loanTermMonths;

                var factor = Math.Pow(1 + r, n);
                monthlyPayment = (decimal)(double.Parse(principal.ToString()) * (r * factor) / (factor - 1));
                totalInterest  = monthlyPayment * loanTermMonths - principal;
            }

            return new LoanCalculation
            {
                CarPrice             = carPrice,
                DownPayment          = downPayment,
                InterestRate         = annualInterestRate,
                LoanTermMonths       = loanTermMonths,
                MonthlyPayment       = Math.Round(monthlyPayment, 2),
                TotalInterestPayment = Math.Round(totalInterest, 2),
                TotalLoanAmount      = Math.Round(monthlyPayment * loanTermMonths, 2),
                CreatedDate          = DateTime.UtcNow
            };
        }

        public async Task<LoanCalculation> SaveAsync(LoanCalculation calculation)
        {
            if (calculation == null) throw new ArgumentNullException(nameof(calculation));

            calculation.CreatedDate = DateTime.UtcNow;

            await _context.LoanCalculations.AddAsync(calculation);
            await _context.SaveChangesAsync();
            return calculation;
        }

        // ADMIN

        public async Task<IList<LoanCalculation>> GetAllAdminAsync()
        {
            return await _context.LoanCalculations
                .AsNoTracking()
                .OrderByDescending(l => l.CreatedDate)
                .ToListAsync();
        }

        public async Task<LoanCalculation?> GetByIdAsync(int id)
        {
            return await _context.LoanCalculations
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task DeleteAsync(int id)
        {
            var calc = await _context.LoanCalculations.FindAsync(id)
                ?? throw new KeyNotFoundException($"Id={id} olan kredit hesablaması tapılmadı.");

            _context.LoanCalculations.Remove(calc);
            await _context.SaveChangesAsync();
        }
    }
}
