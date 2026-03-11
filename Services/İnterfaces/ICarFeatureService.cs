using Car_Project.Models;

namespace Car_Project.Services.Abstractions
{
    /// <summary>
    /// Avtomobil xüsusiyy?tl?ri (CarFeature) üzr? CRUD ?m?liyyatlar? üçün servis interfeysi.
    /// </summary>
    public interface ICarFeatureService
    {
        // ??? PUBLIC (site) metodlar? ???????????????????????????????????????????

        /// <summary>
        /// Bütün aktiv xüsusiyy?tl?ri qaytar?r (checkbox siyah?s? üçün).
        /// </summary>
        Task<IList<CarFeature>> GetAllAsync();

        /// <summary>
        /// Verilmi? avtomobil? aid xüsusiyy?tl?ri qaytar?r.
        /// </summary>
        Task<IList<CarFeature>> GetByCarIdAsync(int carId);

        // ??? ADMIN metodlar? ???????????????????????????????????????????????????

        /// <summary>
        /// Admin paneli üçün bütün xüsusiyy?tl?ri qaytar?r.
        /// </summary>
        Task<IList<CarFeature>> GetAllAdminAsync();

        /// <summary>
        /// Verilmi? id-y? gör? t?k xüsusiyy?ti qaytar?r.
        /// Tap?lmasa null qaytar?r.
        /// </summary>
        Task<CarFeature?> GetByIdAsync(int id);

        /// <summary>
        /// Yeni xüsusiyy?t yarad?r.
        /// </summary>
        Task<CarFeature> CreateAsync(CarFeature feature);

        /// <summary>
        /// Mövcud xüsusiyy?ti yenil?yir.
        /// </summary>
        Task UpdateAsync(CarFeature feature);

        /// <summary>
        /// Xüsusiyy?ti silir. Heç bir avtomobil? ba?l? deyils? silinir, ?ksin? x?ta at?r.
        /// </summary>
        Task DeleteAsync(int id);

        /// <summary>
        /// Bir avtomobilin bütün xüsusiyy?t ?laq?l?rini (CarFeatureMapping) yenil?yir —
        /// köhn? ?laq?l?ri silir, yenil?ri ?lav? edir.
        /// </summary>
        /// <param name="carId">Avtomobilin id-si</param>
        /// <param name="featureIds">Seçilmi? xüsusiyy?tl?rin id siyah?s?</param>
        Task SyncCarFeaturesAsync(int carId, IList<int> featureIds);
    }
}
