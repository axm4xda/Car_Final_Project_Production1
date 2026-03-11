using Car_Project.Data;
using Car_Project.Models;
using Car_Project.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Car_Project.Services
{
    public class CarFeatureService : ICarFeatureService
    {
        private readonly ApplicationDbContext _context;

        public CarFeatureService(ApplicationDbContext context)
        {
            _context = context;
        }

        // PUBLIC

        public async Task<IList<CarFeature>> GetAllAsync()
        {
            return await _context.CarFeatures
                .AsNoTracking()
                .OrderBy(cf => cf.Name)
                .ToListAsync();
        }

        public async Task<IList<CarFeature>> GetByCarIdAsync(int carId)
        {
            return await _context.CarFeatureMappings
                .AsNoTracking()
                .Where(cfm => cfm.CarId == carId)
                .Select(cfm => cfm.CarFeature)
                .OrderBy(cf => cf.Name)
                .ToListAsync();
        }

        // ADMIN

        public async Task<IList<CarFeature>> GetAllAdminAsync()
        {
            return await _context.CarFeatures
                .AsNoTracking()
                .OrderBy(cf => cf.Name)
                .ToListAsync();
        }

        public async Task<CarFeature?> GetByIdAsync(int id)
        {
            return await _context.CarFeatures
                .AsNoTracking()
                .FirstOrDefaultAsync(cf => cf.Id == id);
        }

        public async Task<CarFeature> CreateAsync(CarFeature feature)
        {
            if (feature == null) throw new ArgumentNullException(nameof(feature));

            feature.CreatedDate = DateTime.UtcNow;

            await _context.CarFeatures.AddAsync(feature);
            await _context.SaveChangesAsync();
            return feature;
        }

        public async Task UpdateAsync(CarFeature feature)
        {
            if (feature == null) throw new ArgumentNullException(nameof(feature));

            var existing = await _context.CarFeatures.FindAsync(feature.Id)
                ?? throw new KeyNotFoundException($"Id={feature.Id} olan xüsusiyyət tapılmadı.");

            existing.Name = feature.Name;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var feature = await _context.CarFeatures
                .Include(cf => cf.Cars)
                .FirstOrDefaultAsync(cf => cf.Id == id)
                ?? throw new KeyNotFoundException($"Id={id} olan xüsusiyyət tapılmadı.");

            if (feature.Cars.Any())
                throw new InvalidOperationException(
                    $"'{feature.Name}' xüsusiyyəti {feature.Cars.Count} avtomobilə bağlıdır. " +
                    "Əvvəlcə bağlantıları silin.");

            _context.CarFeatures.Remove(feature);
            await _context.SaveChangesAsync();
        }

        public async Task SyncCarFeaturesAsync(int carId, IList<int> featureIds)
        {
            // Köhnə bütün əlaqələri sil
            var existing = await _context.CarFeatureMappings
                .Where(cfm => cfm.CarId == carId)
                .ToListAsync();

            _context.CarFeatureMappings.RemoveRange(existing);

            // Yeni əlaqələri əlavə et
            if (featureIds != null && featureIds.Count > 0)
            {
                var newMappings = featureIds
                    .Distinct()
                    .Select(fId => new CarFeatureMapping
                    {
                        CarId        = carId,
                        CarFeatureId = fId
                    });

                await _context.CarFeatureMappings.AddRangeAsync(newMappings);
            }

            await _context.SaveChangesAsync();
        }
    }
}
