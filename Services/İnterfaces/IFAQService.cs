using Car_Project.Models;

namespace Car_Project.Services.Abstractions
{
    /// <summary>
    /// Tez-tez veril?n suallar (FAQ) üzr? CRUD ?m?liyyatlar? üçün servis interfeysi.
    /// </summary>
    public interface IFAQService
    {
        // ??? PUBLIC (site) metodlar? ???????????????????????????????????????????

        /// <summary>
        /// Aktiv (IsActive = true) bütün FAQ-lar? Order sah?sin? gör? s?ralanm?? qaytar?r.
        /// </summary>
        Task<IList<FAQ>> GetAllActiveAsync();

        /// <summary>
        /// Aktiv FAQ-lar? GroupName-? gör? qrupla?d?r?lm?? qaytar?r.
        /// Key = GroupName, Value = h?min qrupa aid FAQ siyah?s?.
        /// GroupName null olan FAQ-lar "General" qrupu alt?nda göst?rilir.
        /// </summary>
        Task<IDictionary<string, IList<FAQ>>> GetGroupedAsync();

        // ??? ADMIN metodlar? ???????????????????????????????????????????????????

        /// <summary>
        /// Admin paneli üçün bütün FAQ-lar? (aktiv + deaktiv) qaytar?r.
        /// </summary>
        Task<IList<FAQ>> GetAllAdminAsync();

        /// <summary>
        /// Verilmi? id-y? gör? t?k FAQ-? qaytar?r. Tap?lmasa null qaytar?r.
        /// </summary>
        Task<FAQ?> GetByIdAsync(int id);

        /// <summary>
        /// Yeni FAQ yarad?r.
        /// </summary>
        Task<FAQ> CreateAsync(FAQ faq);

        /// <summary>
        /// Mövcud FAQ-? yenil?yir.
        /// </summary>
        Task UpdateAsync(FAQ faq);

        /// <summary>
        /// FAQ-?n aktiv/deaktiv v?ziyy?tini d?yi?dirir.
        /// </summary>
        Task ToggleActiveAsync(int id);

        /// <summary>
        /// FAQ-? silir.
        /// </summary>
        Task DeleteAsync(int id);
    }
}
