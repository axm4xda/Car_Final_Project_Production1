using Car_Project.Models;

namespace Car_Project.Services.Abstractions
{
    /// <summary>
    /// Mü?t?ri r?yl?ri (Review) üzr? CRUD ?m?liyyatlar? üçün servis interfeysi.
    /// </summary>
    public interface IReviewService
    {
        // ??? PUBLIC (site) metodlar? ???????????????????????????????????????????

        /// <summary>
        /// Yaln?z t?sdiql?nmi? (IsApproved = true) r?yl?ri qaytar?r.
        /// Sayt?n Mü?t?ri R?yl?ri s?hif?sind? istifad? olunur.
        /// </summary>
        Task<IList<Review>> GetApprovedAsync();

        /// <summary>
        /// ?n yüks?k reytinqli N r?yi qaytar?r (Ana s?hif? bölm?si üçün).
        /// </summary>
        /// <param name="count">Qaytar?lacaq r?y say? (default: 6)</param>
        Task<IList<Review>> GetTopRatedAsync(int count = 6);

        // ??? ADMIN metodlar? ???????????????????????????????????????????????????

        /// <summary>
        /// Admin paneli üçün bütün r?yl?ri (t?sdiql?nmi? v? gözl?y?nl?r) qaytar?r.
        /// </summary>
        Task<IList<Review>> GetAllAdminAsync();

        /// <summary>
        /// Verilmi? id-y? gör? t?k r?yi qaytar?r. Tap?lmasa null qaytar?r.
        /// </summary>
        Task<Review?> GetByIdAsync(int id);

        /// <summary>
        /// Yeni r?y yarad?r (IsApproved default olaraq false gelir).
        /// </summary>
        Task<Review> CreateAsync(Review review);

        /// <summary>
        /// Mövcud r?yi yenil?yir.
        /// </summary>
        Task UpdateAsync(Review review);

        /// <summary>
        /// R?yi t?sdiql?yir (IsApproved = true edir).
        /// </summary>
        Task ApproveAsync(int id);

        /// <summary>
        /// R?yi r?dd edir (IsApproved = false edir).
        /// </summary>
        Task RejectAsync(int id);

        /// <summary>
        /// R?yi silir.
        /// </summary>
        Task DeleteAsync(int id);
    }
}
