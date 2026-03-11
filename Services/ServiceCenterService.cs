using Car_Project.Data;
using Car_Project.Models;
using Car_Project.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Car_Project.Services
{
    public class ServiceCenterService : IServiceCenterService
    {
        private readonly ApplicationDbContext _context;

        public ServiceCenterService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ?? PUBLIC ????????????????????????????????????????????????????????????

        public async Task<IList<ServiceCenter>> GetAllAsync()
        {
            return await _context.ServiceCenters
                .AsNoTracking()
                .OrderBy(sc => sc.Name)
                .ToListAsync();
        }

        public async Task<ServiceCenter?> GetByIdAsync(int id)
        {
            return await _context.ServiceCenters
                .AsNoTracking()
                .FirstOrDefaultAsync(sc => sc.Id == id);
        }

        public async Task<IList<ServiceCenter>> GetWithCoordinatesAsync()
        {
            return await _context.ServiceCenters
                .AsNoTracking()
                .Where(sc => sc.Latitude.HasValue && sc.Longitude.HasValue)
                .OrderBy(sc => sc.Name)
                .ToListAsync();
        }

        // ?? ADMIN ?????????????????????????????????????????????????????????????

        public async Task<IList<ServiceCenter>> GetAllAdminAsync()
        {
            return await _context.ServiceCenters
                .AsNoTracking()
                .OrderByDescending(sc => sc.CreatedDate)
                .ToListAsync();
        }

        public async Task<ServiceCenter> CreateAsync(ServiceCenter serviceCenter)
        {
            if (serviceCenter == null) throw new ArgumentNullException(nameof(serviceCenter));

            serviceCenter.CreatedDate = DateTime.UtcNow;

            await _context.ServiceCenters.AddAsync(serviceCenter);
            await _context.SaveChangesAsync();
            return serviceCenter;
        }

        public async Task UpdateAsync(ServiceCenter serviceCenter)
        {
            if (serviceCenter == null) throw new ArgumentNullException(nameof(serviceCenter));

            var existing = await _context.ServiceCenters.FindAsync(serviceCenter.Id)
                ?? throw new KeyNotFoundException($"Id={serviceCenter.Id} olan servis m?rk?zi tap?lmad?.");

            existing.Name         = serviceCenter.Name;
            existing.Address      = serviceCenter.Address;
            existing.Phone        = serviceCenter.Phone;
            existing.Email        = serviceCenter.Email;
            existing.ImageUrl     = serviceCenter.ImageUrl;
            existing.WorkingHours = serviceCenter.WorkingHours;
            existing.Latitude     = serviceCenter.Latitude;
            existing.Longitude    = serviceCenter.Longitude;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var sc = await _context.ServiceCenters.FindAsync(id)
                ?? throw new KeyNotFoundException($"Id={id} olan servis m?rk?zi tap?lmad?.");

            _context.ServiceCenters.Remove(sc);
            await _context.SaveChangesAsync();
        }
    }
}
