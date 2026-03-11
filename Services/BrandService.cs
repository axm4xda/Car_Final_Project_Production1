using Car_Project.Data;
using Car_Project.Models;
using Car_Project.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Car_Project.Services
{
    public class BrandService : IBrandService
    {
        private readonly ApplicationDbContext _context;

        public BrandService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ?? PUBLIC ????????????????????????????????????????????????????????????

        public async Task<IList<Brand>> GetAllAsync()
        {
            return await _context.Brands
                .AsNoTracking()
                .OrderBy(b => b.Name)
                .ToListAsync();
        }

        public async Task<IList<Brand>> GetAllWithCarsAsync()
        {
            return await _context.Brands
                .AsNoTracking()
                .Where(b => b.Cars.Any())
                .Include(b => b.Cars)
                .OrderBy(b => b.Name)
                .ToListAsync();
        }

        public async Task<Brand?> GetByIdAsync(int id)
        {
            return await _context.Brands
                .AsNoTracking()
                .Include(b => b.Cars)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        // ?? ADMIN ?????????????????????????????????????????????????????????????

        public async Task<IList<Brand>> GetAllAdminAsync()
        {
            return await _context.Brands
                .AsNoTracking()
                .OrderBy(b => b.Name)
                .Select(b => new Brand
                {
                    Id           = b.Id,
                    Name         = b.Name,
                    LogoUrl      = b.LogoUrl,
                    CreatedDate  = b.CreatedDate,
                    VehicleCount = b.Cars.Count
                })
                .ToListAsync();
        }

        public async Task<Brand> CreateAsync(Brand brand)
        {
            if (brand == null) throw new ArgumentNullException(nameof(brand));

            if (await ExistsByNameAsync(brand.Name))
                throw new InvalidOperationException($"'{brand.Name}' adlı marka artıq mövcuddur.");

            brand.CreatedDate = DateTime.UtcNow;

            await _context.Brands.AddAsync(brand);
            await _context.SaveChangesAsync();
            return brand;
        }

        public async Task UpdateAsync(Brand brand)
        {
            if (brand == null) throw new ArgumentNullException(nameof(brand));

            var existing = await _context.Brands.FindAsync(brand.Id)
                ?? throw new KeyNotFoundException($"Id={brand.Id} olan marka tapılmadı.");

            if (await ExistsByNameAsync(brand.Name, excludeId: brand.Id))
                throw new InvalidOperationException($"'{brand.Name}' adlı marka artıq mövcuddur.");

            existing.Name    = brand.Name;
            existing.LogoUrl = brand.LogoUrl;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var brand = await _context.Brands
                .Include(b => b.Cars)
                .FirstOrDefaultAsync(b => b.Id == id)
                ?? throw new KeyNotFoundException($"Id={id} olan marka tapılmadı.");

            if (brand.Cars.Any())
                throw new InvalidOperationException(
                    $"'{brand.Name}' markasına bağlı {brand.Cars.Count} avtomobil var. " +
                    "Əvvəlcə avtomobilləri silin.");

            _context.Brands.Remove(brand);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;

            var query = _context.Brands.Where(b => b.Name == name);

            if (excludeId.HasValue)
                query = query.Where(b => b.Id != excludeId.Value);

            return await query.AnyAsync();
        }
    }
}
