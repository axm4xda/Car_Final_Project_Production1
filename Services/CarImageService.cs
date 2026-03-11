using Car_Project.Data;
using Car_Project.Models;
using Car_Project.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Car_Project.Services
{
    public class CarImageService : ICarImageService
    {
        private readonly ApplicationDbContext _context;

        public CarImageService(ApplicationDbContext context)
        {
            _context = context;
        }

        // PUBLIC

        public async Task<IList<CarImage>> GetByCarIdAsync(int carId)
        {
            return await _context.CarImages
                .AsNoTracking()
                .Where(ci => ci.CarId == carId)
                .OrderBy(ci => ci.Order)
                .ToListAsync();
        }

        public async Task<CarImage?> GetMainImageAsync(int carId)
        {
            return await _context.CarImages
                .AsNoTracking()
                .FirstOrDefaultAsync(ci => ci.CarId == carId && ci.IsMain);
        }

        // ADMIN

        public async Task<CarImage> AddAsync(CarImage carImage)
        {
            if (carImage == null) throw new ArgumentNullException(nameof(carImage));

            // Bu avtomobilin ilk şəkilidisə əsas şəkil et
            var hasAny = await _context.CarImages.AnyAsync(ci => ci.CarId == carImage.CarId);
            if (!hasAny)
                carImage.IsMain = true;

            // Order avtomatik: mövcud maksimum + 1
            var maxOrder = await _context.CarImages
                .Where(ci => ci.CarId == carImage.CarId)
                .Select(ci => (int?)ci.Order)
                .MaxAsync() ?? 0;

            carImage.Order       = maxOrder + 1;
            carImage.CreatedDate = DateTime.UtcNow;

            await _context.CarImages.AddAsync(carImage);
            await _context.SaveChangesAsync();
            return carImage;
        }

        public async Task AddManyAsync(IList<CarImage> carImages)
        {
            if (carImages == null || carImages.Count == 0)
                throw new ArgumentException("Şəkil siyahısı boş ola bilməz.", nameof(carImages));

            // Mövcud şəkil sayı
            var carId    = carImages.First().CarId;
            var hasAny   = await _context.CarImages.AnyAsync(ci => ci.CarId == carId);
            var maxOrder = await _context.CarImages
                .Where(ci => ci.CarId == carId)
                .Select(ci => (int?)ci.Order)
                .MaxAsync() ?? 0;

            for (int i = 0; i < carImages.Count; i++)
            {
                carImages[i].Order       = maxOrder + i + 1;
                carImages[i].CreatedDate = DateTime.UtcNow;

                // İlk şəkil əsas olsun (əgər hələ heç biri yoxdursa)
                if (!hasAny && i == 0)
                    carImages[i].IsMain = true;
            }

            await _context.CarImages.AddRangeAsync(carImages);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateOrderAsync(int imageId, int newOrder)
        {
            var image = await _context.CarImages.FindAsync(imageId)
                ?? throw new KeyNotFoundException($"Id={imageId} olan şəkil tapılmadı.");

            image.Order = newOrder;
            await _context.SaveChangesAsync();
        }

        public async Task SetMainImageAsync(int imageId)
        {
            var image = await _context.CarImages.FindAsync(imageId)
                ?? throw new KeyNotFoundException($"Id={imageId} olan şəkil tapılmadı.");

            // Eyni avtomobilin bütün şəkillərini əsas olmayan et
            var others = await _context.CarImages
                .Where(ci => ci.CarId == image.CarId && ci.IsMain)
                .ToListAsync();

            foreach (var other in others)
                other.IsMain = false;

            image.IsMain = true;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int imageId)
        {
            var image = await _context.CarImages.FindAsync(imageId)
                ?? throw new KeyNotFoundException($"Id={imageId} olan şəkil tapılmadı.");

            _context.CarImages.Remove(image);
            await _context.SaveChangesAsync();

            // Əgər əsas şəkil silindisə, növbəti şəkili əsas et
            if (image.IsMain)
            {
                var next = await _context.CarImages
                    .Where(ci => ci.CarId == image.CarId)
                    .OrderBy(ci => ci.Order)
                    .FirstOrDefaultAsync();

                if (next != null)
                {
                    next.IsMain = true;
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task DeleteAllByCarIdAsync(int carId)
        {
            var images = await _context.CarImages
                .Where(ci => ci.CarId == carId)
                .ToListAsync();

            if (images.Count > 0)
            {
                _context.CarImages.RemoveRange(images);
                await _context.SaveChangesAsync();
            }
        }
    }
}
