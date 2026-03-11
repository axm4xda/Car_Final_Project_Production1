using Car_Project.Models;

namespace Car_Project.Services.Abstractions
{
    /// <summary>
    /// ?laq? formu mesajlar? (ContactMessage) üzr? ?m?liyyatlar üçün servis interfeysi.
    /// </summary>
    public interface IContactMessageService
    {
        // ??? PUBLIC (site) metodlar? ???????????????????????????????????????????

        /// <summary>
        /// Saytdan gönd?ril?n yeni ?laq? mesaj?n? bazaya yaz?r.
        /// </summary>
        Task<ContactMessage> SendAsync(ContactMessage message);

        // ??? ADMIN metodlar? ???????????????????????????????????????????????????

        /// <summary>
        /// Admin paneli üçün bütün mesajlar? ?n yeni ?vv?l s?ralanm?? qaytar?r.
        /// </summary>
        Task<IList<ContactMessage>> GetAllAdminAsync();

        /// <summary>
        /// Yaln?z oxunmam?? (IsRead = false) mesajlar? qaytar?r.
        /// Admin nav-bar bildiri? sayac? üçün istifad? olunur.
        /// </summary>
        Task<IList<ContactMessage>> GetUnreadAsync();

        /// <summary>
        /// Oxunmam?? mesajlar?n say?n? qaytar?r (bildiri? badge-i üçün).
        /// </summary>
        Task<int> GetUnreadCountAsync();

        /// <summary>
        /// Verilmi? id-y? gör? t?k mesaj? qaytar?r. Tap?lmasa null qaytar?r.
        /// </summary>
        Task<ContactMessage?> GetByIdAsync(int id);

        /// <summary>
        /// Mesaj? oxunmu? olaraq i?ar?l?yir (IsRead = true).
        /// </summary>
        Task MarkAsReadAsync(int id);

        /// <summary>
        /// Bütün oxunmam?? mesajlar? oxunmu? olaraq i?ar?l?yir.
        /// </summary>
        Task MarkAllAsReadAsync();

        /// <summary>
        /// Mesaj? silir.
        /// </summary>
        Task DeleteAsync(int id);
    }
}
