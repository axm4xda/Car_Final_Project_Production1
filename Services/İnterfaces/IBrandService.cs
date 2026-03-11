using Car_Project.Models;

namespace Car_Project.Services.Abstractions
{
    /// <summary>
    /// Avtomobil markas? (Brand) üzr? CRUD ?m?liyyatlar? üçün servis interfeysi.
    /// </summary>
    public interface IBrandService
    {
        // ??? PUBLIC (site) metodlar? ???????????????????????????????????????????

        /// <summary>
        /// Bütün markalar? qaytar?r.
        /// Filtrasiya panelind?ki marka siyah?s?nda istifad? olunur.
        /// </summary>
        Task<IList<Brand>> GetAllAsync();

        /// <summary>
        /// Yaln?z en az bir aktiv avtomobili olan markalar? qaytar?r.
        /// </summary>
        Task<IList<Brand>> GetAllWithCarsAsync();

        /// <summary>
        /// Verilmi? id-y? gör? t?k markan? Cars kolleksiyas? il? birlikd? qaytar?r.
        /// Tap?lmasa null qaytar?r.
        /// </summary>
        Task<Brand?> GetByIdAsync(int id);

        // ??? ADMIN metodlar? ???????????????????????????????????????????????????

        /// <summary>
        /// Admin paneli üçün bütün markalar? avtomobil saylar? il? birlikd? qaytar?r.
        /// </summary>
        Task<IList<Brand>> GetAllAdminAsync();

        /// <summary>
        /// Yeni marka yarad?r.
        /// </summary>
        Task<Brand> CreateAsync(Brand brand);

        /// <summary>
        /// Mövcud markan?n m?lumatlar?n? yenil?yir.
        /// </summary>
        Task UpdateAsync(Brand brand);

        /// <summary>
        /// Markan? silir. ?g?r markaya ba?l? avtomobil varsa x?ta at?r.
        /// </summary>
        Task DeleteAsync(int id);

        /// <summary>
        /// Verilmi? adda markan?n art?q mövcud olub-olmad???n? yoxlay?r (unikall?q üçün).
        /// </summary>
        /// <param name="name">Yoxlan?lacaq marka ad?</param>
        /// <param name="excludeId">Yenil?m? zaman? öz id-sini istisna etm?k üçün (null = yeni yaratma)</param>
        Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
    }
}
