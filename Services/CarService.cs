using Car_Project.Data;
using Car_Project.Models;
using Car_Project.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Car_Project.Services
{
    public class CarService : ICarService
    {
        private readonly ApplicationDbContext _context;

        public CarService(ApplicationDbContext context)
        {
            _context = context;
        }

        // PUBLIC

        public async Task<IList<Car>> GetAllAsync()
        {
            return await _context.Cars
                .AsNoTracking()
                .Where(c => c.IsApproved)
                .Include(c => c.Brand)
                .Include(c => c.Images.OrderBy(i => i.Order))
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();
        }

        public async Task<IList<Car>> GetFilteredAsync(
            int? brandId,
            CarCondition? condition,
            FuelType? fuelType,
            TransmissionType? transmission,
            decimal? minPrice,
            decimal? maxPrice,
            int? minYear,
            int? maxYear,
            string? bodyStyle = null)
        {
            var query = _context.Cars
                .AsNoTracking()
                .Where(c => c.IsApproved)
                .Include(c => c.Brand)
                .Include(c => c.Images.OrderBy(i => i.Order))
                .AsQueryable();

            if (brandId.HasValue)
                query = query.Where(c => c.BrandId == brandId.Value);

            if (condition.HasValue)
                query = query.Where(c => c.Condition == condition.Value);

            if (fuelType.HasValue)
                query = query.Where(c => c.FuelType == fuelType.Value);

            if (transmission.HasValue)
                query = query.Where(c => c.Transmission == transmission.Value);

            if (minPrice.HasValue)
                query = query.Where(c => c.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(c => c.Price <= maxPrice.Value);

            if (minYear.HasValue)
                query = query.Where(c => c.Year >= minYear.Value);

            if (maxYear.HasValue)
                query = query.Where(c => c.Year <= maxYear.Value);

            if (!string.IsNullOrEmpty(bodyStyle))
                query = query.Where(c => c.BodyStyle != null && c.BodyStyle.ToLower() == bodyStyle.ToLower());

            // VIP listings always appear at the top
            return await query
                .OrderByDescending(c => c.ListingType == ListingType.VIP)
                .ThenByDescending(c => c.CreatedDate)
                .ToListAsync();
        }

        public async Task<Car?> GetByIdAsync(int id)
        {
            return await _context.Cars
                .AsNoTracking()
                .Where(c => c.IsApproved)
                .Include(c => c.Brand)
                .Include(c => c.Images.OrderBy(i => i.Order))
                .Include(c => c.Features)
                    .ThenInclude(f => f.CarFeature)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IList<Car>> GetLatestAsync(int count = 6)
        {
            return await _context.Cars
                .AsNoTracking()
                .Where(c => c.IsApproved)
                .Include(c => c.Brand)
                .Include(c => c.Images.OrderBy(i => i.Order))
                .OrderByDescending(c => c.CreatedDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IList<Car>> GetTrendingAsync(int count = 6)
        {
            return await _context.Cars
                .AsNoTracking()
                .Where(c => c.IsApproved)
                .Include(c => c.Brand)
                .Include(c => c.Images.OrderBy(i => i.Order))
                .OrderByDescending(c => c.CreatedDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IList<Car>> GetRelatedAsync(int brandId, int excludeCarId, int count = 4)
        {
            return await _context.Cars
                .AsNoTracking()
                .Where(c => c.IsApproved)
                .Include(c => c.Brand)
                .Include(c => c.Images.OrderBy(i => i.Order))
                .Where(c => c.BrandId == brandId && c.Id != excludeCarId)
                .OrderByDescending(c => c.CreatedDate)
                .Take(count)
                .ToListAsync();
        }

        // ADMIN

        public async Task<IList<Car>> GetAllAdminAsync()
        {
            return await _context.Cars
                .AsNoTracking()
                .Include(c => c.Brand)
                .Include(c => c.Images.OrderBy(i => i.Order))
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();
        }

        public async Task<(IList<Car> Items, int TotalCount)> GetPagedAdminAsync(
            int page, int pageSize = 10)
        {
            if (page < 1) page = 1;

            var query = _context.Cars
                .AsNoTracking()
                .Include(c => c.Brand)
                .Include(c => c.Images.OrderBy(i => i.Order))
                .OrderByDescending(c => c.CreatedDate);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<Car> CreateAsync(Car car)
        {
            if (car == null) throw new ArgumentNullException(nameof(car));

            car.CreatedDate = DateTime.UtcNow;

            await _context.Cars.AddAsync(car);
            await _context.SaveChangesAsync();
            return car;
        }

        public async Task UpdateAsync(Car car)
        {
            if (car == null) throw new ArgumentNullException(nameof(car));

            var existing = await _context.Cars.FindAsync(car.Id)
                ?? throw new KeyNotFoundException($"Id={car.Id} olan avtomobil tapılmadı.");

            existing.Title          = car.Title;
            existing.Price          = car.Price;
            existing.MonthlyPayment = car.MonthlyPayment;
            existing.Year           = car.Year;
            existing.Mileage        = car.Mileage;
            existing.FuelType       = car.FuelType;
            existing.Transmission   = car.Transmission;
            existing.Condition      = car.Condition;
            existing.BodyStyle      = car.BodyStyle;
            existing.DriveType      = car.DriveType;
            existing.Color          = car.Color;
            existing.InteriorColor  = car.InteriorColor;
            existing.Cylinders      = car.Cylinders;
            existing.DoorCount      = car.DoorCount;
            existing.Description    = car.Description;
            existing.Badge          = car.Badge;
            existing.BadgeColor     = car.BadgeColor;
            existing.BrandId        = car.BrandId;
            existing.ThumbnailUrl   = car.ThumbnailUrl;
            existing.IsApproved     = car.IsApproved;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var car = await _context.Cars.FindAsync(id)
                ?? throw new KeyNotFoundException($"Id={id} olan avtomobil tapılmadı.");

            _context.Cars.Remove(car);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Cars.AnyAsync(c => c.Id == id);
        }
    }
}
