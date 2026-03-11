using Car_Project.Models;

namespace Car_Project.Services.Abstractions
{
    /// <summary>
    /// Servis m?rk?zl?ri (ServiceCenter) üzr? CRUD ?m?liyyatlar? üçün servis interfeysi.
    /// </summary>
    public interface IServiceCenterService
    {
        // ??? PUBLIC (site) metodlar? ???????????????????????????????????????????

        /// <summary>
        /// Bütün servis m?rk?zl?rini qaytar?r.
        /// Servis M?rk?zi s?hif?sind?ki x?rit? v? siyah? üçün istifad? olunur.
        /// </summary>
        Task<IList<ServiceCenter>> GetAllAsync();

        /// <summary>
        /// Verilmi? id-y? gör? t?k servis m?rk?zini qaytar?r. Tap?lmasa null qaytar?r.
        /// </summary>
        Task<ServiceCenter?> GetByIdAsync(int id);

        /// <summary>
        /// Koordinatlar? olan bütün servis m?rk?zl?rini qaytar?r (x?rit? pinl?ri üçün).
        /// </summary>
        Task<IList<ServiceCenter>> GetWithCoordinatesAsync();

        // ??? ADMIN metodlar? ???????????????????????????????????????????????????

        /// <summary>
        /// Admin paneli üçün bütün servis m?rk?zl?rini qaytar?r.
        /// </summary>
        Task<IList<ServiceCenter>> GetAllAdminAsync();

        /// <summary>
        /// Yeni servis m?rk?zi yarad?r.
        /// </summary>
        Task<ServiceCenter> CreateAsync(ServiceCenter serviceCenter);

        /// <summary>
        /// Mövcud servis m?rk?zinin m?lumatlar?n? yenil?yir.
        /// </summary>
        Task UpdateAsync(ServiceCenter serviceCenter);

        /// <summary>
        /// Servis m?rk?zini silir.
        /// </summary>
        Task DeleteAsync(int id);
    }
}
