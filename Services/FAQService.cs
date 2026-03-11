using Car_Project.Data;
using Car_Project.Models;
using Car_Project.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Car_Project.Services
{
    public class FAQService : IFAQService
    {
        private readonly ApplicationDbContext _context;

        public FAQService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ?? PUBLIC ????????????????????????????????????????????????????????????

        public async Task<IList<FAQ>> GetAllActiveAsync()
        {
            return await _context.FAQs
                .AsNoTracking()
                .Where(f => f.IsActive)
                .OrderBy(f => f.Order)
                .ThenBy(f => f.GroupName)
                .ToListAsync();
        }

        public async Task<IDictionary<string, IList<FAQ>>> GetGroupedAsync()
        {
            var faqs = await GetAllActiveAsync();

            return faqs
                .GroupBy(f => f.GroupName ?? "General")
                .ToDictionary(
                    g => g.Key,
                    g => (IList<FAQ>)g.OrderBy(f => f.Order).ToList());
        }

        // ?? ADMIN ?????????????????????????????????????????????????????????????

        public async Task<IList<FAQ>> GetAllAdminAsync()
        {
            return await _context.FAQs
                .AsNoTracking()
                .OrderBy(f => f.GroupName)
                .ThenBy(f => f.Order)
                .ToListAsync();
        }

        public async Task<FAQ?> GetByIdAsync(int id)
        {
            return await _context.FAQs
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<FAQ> CreateAsync(FAQ faq)
        {
            if (faq == null) throw new ArgumentNullException(nameof(faq));

            // Order avtomatik: eyni qrupdak? max + 1
            var maxOrder = await _context.FAQs
                .Where(f => f.GroupName == faq.GroupName)
                .Select(f => (int?)f.Order)
                .MaxAsync() ?? 0;

            faq.Order       = maxOrder + 1;
            faq.CreatedDate = DateTime.UtcNow;

            await _context.FAQs.AddAsync(faq);
            await _context.SaveChangesAsync();
            return faq;
        }

        public async Task UpdateAsync(FAQ faq)
        {
            if (faq == null) throw new ArgumentNullException(nameof(faq));

            var existing = await _context.FAQs.FindAsync(faq.Id)
                ?? throw new KeyNotFoundException($"Id={faq.Id} olan FAQ tapılmadı.");

            existing.Question  = faq.Question;
            existing.Answer    = faq.Answer;
            existing.GroupName = faq.GroupName;
            existing.Order     = faq.Order;
            existing.IsActive  = faq.IsActive;

            await _context.SaveChangesAsync();
        }

        public async Task ToggleActiveAsync(int id)
        {
            var faq = await _context.FAQs.FindAsync(id)
                ?? throw new KeyNotFoundException($"Id={id} olan FAQ tapılmadı.");

            faq.IsActive = !faq.IsActive;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var faq = await _context.FAQs.FindAsync(id)
                ?? throw new KeyNotFoundException($"Id={id} olan FAQ tapılmadı.");

            _context.FAQs.Remove(faq);
            await _context.SaveChangesAsync();
        }
    }
}
