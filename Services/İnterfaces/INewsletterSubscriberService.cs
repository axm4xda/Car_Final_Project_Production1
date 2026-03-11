using Car_Project.Models;

namespace Car_Project.Services.Abstractions
{
    /// <summary>
    /// X?b?r bülleteni abun?çil?ri (NewsletterSubscriber) üzr? ?m?liyyatlar üçün servis interfeysi.
    /// </summary>
    public interface INewsletterSubscriberService
    {
        // ??? PUBLIC (site) metodlar? ???????????????????????????????????????????

        /// <summary>
        /// Verilmi? e-poçt ünvan?n? bülleten? abun? edir.
        /// Eyni e-poçt art?q mövcuddursa abun?ni yenid?n aktivl??dirir (IsActive = true).
        /// </summary>
        /// <param name="email">Abun? olunacaq e-poçt ünvan?</param>
        Task SubscribeAsync(string email);

        /// <summary>
        /// Verilmi? e-poçt ünvan?n?n abun?sini l??v edir (IsActive = false).
        /// </summary>
        /// <param name="email">Abun?si l??v edil?c?k e-poçt ünvan?</param>
        Task UnsubscribeAsync(string email);

        /// <summary>
        /// Verilmi? e-poçt ünvan?n?n aktiv abun?çi olub-olmad???n? yoxlay?r.
        /// </summary>
        Task<bool> IsSubscribedAsync(string email);

        // ??? ADMIN metodlar? ???????????????????????????????????????????????????

        /// <summary>
        /// Admin paneli üçün bütün abun?çil?ri (aktiv + deaktiv) qaytar?r.
        /// </summary>
        Task<IList<NewsletterSubscriber>> GetAllAdminAsync();

        /// <summary>
        /// Yaln?z aktiv (IsActive = true) abun?çil?ri qaytar?r (bulk e-poçt gönd?rm?k üçün).
        /// </summary>
        Task<IList<NewsletterSubscriber>> GetActiveAsync();

        /// <summary>
        /// Aktiv abun?çil?rin say?n? qaytar?r.
        /// </summary>
        Task<int> GetActiveCountAsync();

        /// <summary>
        /// Abun?çini bazadan tamamil? silir.
        /// </summary>
        Task DeleteAsync(int id);
    }
}
