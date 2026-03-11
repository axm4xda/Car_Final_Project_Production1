using Car_Project.Data;
using Car_Project.Models;
using Car_Project.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Car_Project.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _context;

        public PaymentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Payment> ProcessAsync(Payment payment)
        {
            if (payment == null) throw new ArgumentNullException(nameof(payment));

            payment.CreatedDate = DateTime.UtcNow;

            // Kart nömr?sinin yaln?z son 4 r?q?mini saxla
            if (!string.IsNullOrWhiteSpace(payment.CardLastFour) &&
                payment.CardLastFour.Length > 4)
            {
                payment.CardLastFour = payment.CardLastFour[^4..];
            }

            // Na?d öd?ni?d? kartla ba?l? sah?l?ri t?mizl?
            if (payment.Method == PaymentMethod.CashOnDelivery)
            {
                payment.CardHolderName = null;
                payment.CardLastFour   = null;
                payment.Status         = PaymentStatus.Pending;
            }
            else
            {
                // Real öd?ni? gateway inteqrasiyas? burada olacaq
                // Haz?rda u?urlu say?l?r
                payment.Status        = PaymentStatus.Paid;
                payment.PaidAt        = DateTime.UtcNow;
                payment.TransactionId = $"TXN-{Guid.NewGuid():N}"[..18];
            }

            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();
            return payment;
        }

        /// <summary>
        /// Persists the final payment amount after the order total is calculated.
        /// </summary>
        public async Task UpdateAmountAsync(int paymentId, decimal amount)
        {
            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment != null)
            {
                payment.Amount = amount;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Payment?> GetByIdAsync(int id) =>
            await _context.Payments
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

        public async Task<IList<Payment>> GetAllAdminAsync() =>
            await _context.Payments
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();

        public async Task<(IList<Payment> Items, int TotalCount)> GetPagedAdminAsync(
            int page, int pageSize = 15)
        {
            if (page < 1) page = 1;
            var query = _context.Payments
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedDate);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return (items, totalCount);
        }

        public async Task RefundAsync(int paymentId)
        {
            var payment = await _context.Payments.FindAsync(paymentId)
                ?? throw new KeyNotFoundException($"Id={paymentId} olan öd?ni? tap?lmad?.");

            if (payment.Status != PaymentStatus.Paid)
                throw new InvalidOperationException("Yaln?z tamamlanm?? öd?ni?l?r geri qaytar?la bil?r.");

            // Real gateway geri qaytarma buraya ?lav? olunacaq
            payment.Status = PaymentStatus.Refunded;
            await _context.SaveChangesAsync();
        }
    }
}
