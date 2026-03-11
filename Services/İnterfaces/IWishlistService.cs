using Car_Project.Models;

namespace Car_Project.Services.Abstractions
{
    public interface IWishlistService
    {
        Task<IList<Car>> GetWishlistAsync(string sessionId);
        Task<int> GetCountAsync(string sessionId);
        Task AddAsync(string sessionId, int carId);
        Task RemoveAsync(string sessionId, int carId);
        Task ClearAsync(string sessionId);
        Task<bool> IsInWishlistAsync(string sessionId, int carId);
    }
}
