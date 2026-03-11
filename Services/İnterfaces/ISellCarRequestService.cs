using Car_Project.Models;

namespace Car_Project.Services.Abstractions
{
    /// <summary>
    /// Avtomobil sat?? müraci?tl?ri (SellCarRequest) üzr? ?m?liyyatlar üçün servis interfeysi.
    /// </summary>
    public interface ISellCarRequestService
    {
        // ??? PUBLIC (site) metodlar? ???????????????????????????????????????????

        /// <summary>
        /// ?stifad?çinin "Avtomobilini Sat" formas?ndan gönd?rdiyi müraci?ti bazaya yaz?r.
        /// IsReviewed avtomatik olaraq false t?yin edilir.
        /// </summary>
        Task<SellCarRequest> SubmitAsync(SellCarRequest request);

        // ??? ADMIN metodlar? ???????????????????????????????????????????????????

        /// <summary>
        /// Admin paneli üçün bütün müraci?tl?ri ?n yeni ?vv?l s?ralanm?? qaytar?r.
        /// </summary>
        Task<IList<SellCarRequest>> GetAllAdminAsync();

        /// <summary>
        /// Yaln?z h?l? n?z?rd?n keçirilm?mi? (IsReviewed = false) müraci?tl?ri qaytar?r.
        /// </summary>
        Task<IList<SellCarRequest>> GetPendingAsync();

        /// <summary>
        /// Gözl?y?n müraci?tl?rin say?n? qaytar?r (admin bildiri? sayac? üçün).
        /// </summary>
        Task<int> GetPendingCountAsync();

        /// <summary>
        /// Verilmi? id-y? gör? t?k müraci?ti qaytar?r. Tap?lmasa null qaytar?r.
        /// </summary>
        Task<SellCarRequest?> GetByIdAsync(int id);

        /// <summary>
        /// Müraci?ti n?z?rd?n keçirilmi? olaraq i?ar?l?yir (IsReviewed = true).
        /// </summary>
        Task MarkAsReviewedAsync(int id);

        /// <summary>
        /// Müraci?ti silir.
        /// </summary>
        Task DeleteAsync(int id);
    }
}
