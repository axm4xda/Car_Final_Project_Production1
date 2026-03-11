using Car_Project.Models;

namespace Car_Project.Services.Abstractions
{
    /// <summary>
    /// Avtomobil ??kill?ri (CarImage) üzr? ?m?liyyatlar üçün servis interfeysi.
    /// ??kill?rin yükl?nm?si IFileService il? birlikd? istifad? olunur.
    /// </summary>
    public interface ICarImageService
    {
        // ??? PUBLIC (site) metodlar? ???????????????????????????????????????????

        /// <summary>
        /// Verilmi? avtomobil? aid bütün ??kill?ri s?ra nömr?sin? (Order) gör? qaytar?r.
        /// </summary>
        Task<IList<CarImage>> GetByCarIdAsync(int carId);

        /// <summary>
        /// Verilmi? avtomobilin ?sas (IsMain = true) ??klini qaytar?r.
        /// ?sas ??kil yoxdursa null qaytar?r.
        /// </summary>
        Task<CarImage?> GetMainImageAsync(int carId);

        // ??? ADMIN metodlar? ???????????????????????????????????????????????????

        /// <summary>
        /// Avtomobil? yeni ??kil ?lav? edir.
        /// ?g?r bu ilk ??kildirs? avtomatik olaraq ?sas (IsMain = true) t?yin edilir.
        /// </summary>
        /// <param name="carImage">?lav? edil?c?k CarImage obyekti (ImageUrl art?q doldurulmu? olmal?d?r)</param>
        Task<CarImage> AddAsync(CarImage carImage);

        /// <summary>
        /// Bir neç? ??kli eyni avtomobil? toplu ??kild? ?lav? edir.
        /// </summary>
        Task AddManyAsync(IList<CarImage> carImages);

        /// <summary>
        /// ??klin s?ra nömr?sini (Order) yenil?yir — qalereyada s?ra d?yi?ikliyi üçün.
        /// </summary>
        Task UpdateOrderAsync(int imageId, int newOrder);

        /// <summary>
        /// Verilmi? ??kli ?sas (IsMain) t?yin edir;
        /// eyni avtomobilin dig?r ??kill?rind?n IsMain = false edilir.
        /// </summary>
        Task SetMainImageAsync(int imageId);

        /// <summary>
        /// ??kli veril?nl?r bazas?ndan silir.
        /// Fiziki fayl? silm?k üçün IFileService.Delete() ayr?ca ça??r?lmal?d?r.
        /// </summary>
        Task DeleteAsync(int imageId);

        /// <summary>
        /// Bir avtomobil? aid bütün ??kill?ri silir (avtomobil silin?rk?n istifad? olunur).
        /// </summary>
        Task DeleteAllByCarIdAsync(int carId);
    }
}
