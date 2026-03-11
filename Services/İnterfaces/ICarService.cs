using Car_Project.Models;

namespace Car_Project.Services.Abstractions
{
    /// <summary>
    /// Avtomobil (Car) üzr? bütün CRUD v? filtrasiya ?m?liyyatlar? üçün servis interfeysi.
    /// </summary>
    public interface ICarService
    {
        // ??? PUBLIC (site) metodlar? ???????????????????????????????????????????

        /// <summary>
        /// Aktiv (silinm?mi?) bütün avtomobill?ri Brand v? ?sas ??kli il? birlikd? qaytar?r.
        /// Site?n siyah? s?hif?sind? istifad? olunur.
        /// </summary>
        Task<IList<Car>> GetAllAsync();

        /// <summary>
        /// Verilmi? filtrl?r? uy?un avtomobill?ri qaytar?r (qiym?t, marka, yanacaq növü v? s.).
        /// </summary>
        /// <param name="brandId">Marka filtri (null = ham?s?)</param>
        /// <param name="condition">V?ziyy?t filtri: New / Used (null = ham?s?)</param>
        /// <param name="fuelType">Yanacaq növü filtri (null = ham?s?)</param>
        /// <param name="transmission">Sür?tl?r qutusu filtri (null = ham?s?)</param>
        /// <param name="minPrice">Minimum qiym?t (null = limitsiz)</param>
        /// <param name="maxPrice">Maksimum qiym?t (null = limitsiz)</param>
        /// <param name="minYear">Minimum il (null = limitsiz)</param>
        /// <param name="maxYear">Maksimum il (null = limitsiz)</param>
        /// <param name="bodyStyle">Karoseriya üslubu filtri (null = hamıs?)</param>
        Task<IList<Car>> GetFilteredAsync(
            int? brandId,
            CarCondition? condition,
            FuelType? fuelType,
            TransmissionType? transmission,
            decimal? minPrice,
            decimal? maxPrice,
            int? minYear,
            int? maxYear,
            string? bodyStyle = null);

        /// <summary>
        /// Verilmi? id-y? gör? t?k avtomobili bütün ?laq?li m?lumatlar? il? (Brand, Images, Features) qaytar?r.
        /// Tap?lmasa null qaytar?r.
        /// </summary>
        Task<Car?> GetByIdAsync(int id);

        /// <summary>
        /// ?n son ?lav? edilmi? N avtomobili qaytar?r (Ana s?hif? "Yeni Elanlar" bölm?si).
        /// </summary>
        /// <param name="count">Qaytar?lacaq avtomobil say? (default: 6)</param>
        Task<IList<Car>> GetLatestAsync(int count = 6);

        /// <summary>
        /// Trending bölm?si üçün h?r iki condition-dan (New + Used) ma??nlar? qaytar.
        /// </summary>
        Task<IList<Car>> GetTrendingAsync(int count = 6);

        /// <summary>
        /// Eyni markal? dig?r avtomobill?ri qaytar?r — detal s?hif?sind?ki "Ox?ar Elanlar" bölm?si.
        /// </summary>
        /// <param name="brandId">Marka id-si</param>
        /// <param name="excludeCarId">Haz?rda bax?lan avtomobilin id-si (n?tic?d?n ç?xar?l?r)</param>
        /// <param name="count">Qaytar?lacaq say (default: 4)</param>
        Task<IList<Car>> GetRelatedAsync(int brandId, int excludeCarId, int count = 4);

        // ??? ADMIN metodlar? ???????????????????????????????????????????????????

        /// <summary>
        /// Admin paneli üçün bütün avtomobill?ri (silinmi?l?r daxil olmaqla) qaytar?r.
        /// S?ralama: ?n yeni ?vv?l.
        /// </summary>
        Task<IList<Car>> GetAllAdminAsync();

        /// <summary>
        /// Admin paneli üçün s?hif?l?m? il? avtomobil siyah?s? qaytar?r.
        /// </summary>
        /// <param name="page">Cari s?hif? nömr?si (1-d?n ba?lay?r)</param>
        /// <param name="pageSize">H?r s?hif?d? neç? avtomobil (default: 10)</param>
        Task<(IList<Car> Items, int TotalCount)> GetPagedAdminAsync(int page, int pageSize = 10);

        /// <summary>
        /// Yeni avtomobil yarad?r v? veril?nl?r bazas?na ?lav? edir.
        /// </summary>
        /// <param name="car">?lav? edil?c?k Car obyekti</param>
        Task<Car> CreateAsync(Car car);

        /// <summary>
        /// Mövcud avtomobilin m?lumatlar?n? yenil?yir.
        /// </summary>
        /// <param name="car">Yenil?n?c?k Car obyekti (Id mütl?q olmal?d?r)</param>
        Task UpdateAsync(Car car);

        /// <summary>
        /// Avtomobili veril?nl?r bazas?ndan tamamil? silir (hard delete).
        /// </summary>
        /// <param name="id">Silin?c?k avtomobilin id-si</param>
        Task DeleteAsync(int id);

        /// <summary>
        /// Verilmi? id-li avtomobilin mövcud olub-olmad???n? yoxlay?r.
        /// </summary>
        Task<bool> ExistsAsync(int id);
    }
}
