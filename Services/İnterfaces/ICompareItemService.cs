using Car_Project.Models;

namespace Car_Project.Services.Abstractions
{
    /// <summary>
    /// Müqayis? siyah?s? (CompareItem) üzr? session-?sasl? ?m?liyyatlar üçün servis interfeysi.
    /// H?r istifad?çi öz session ID-sin? gör? maksimum 4 avtomobil müqayis? ed? bil?r.
    /// </summary>
    public interface ICompareItemService
    {
        // ??? PUBLIC (site) metodlar? ???????????????????????????????????????????

        /// <summary>
        /// Verilmi? session-a aid müqayis? siyah?s?ndak? avtomobill?ri
        /// bütün detallar? (Brand, Images, Features) il? birlikd? qaytar?r.
        /// </summary>
        /// <param name="sessionId">?stifad?çinin session ID-si</param>
        Task<IList<Car>> GetCompareListAsync(string sessionId);

        /// <summary>
        /// Müqayis? siyah?s?ndak? avtomobil say?n? qaytar?r (nav-bar sayac? üçün).
        /// </summary>
        Task<int> GetCountAsync(string sessionId);

        /// <summary>
        /// Müqayis? siyah?s?na avtomobil ?lav? edir.
        /// Siyah?da art?q varsa v? ya 4 limiti dolubsa x?ta at?r.
        /// </summary>
        /// <param name="sessionId">?stifad?çinin session ID-si</param>
        /// <param name="carId">?lav? edil?c?k avtomobilin id-si</param>
        Task AddAsync(string sessionId, int carId);

        /// <summary>
        /// Müqayis? siyah?s?ndan mü?yy?n avtomobili ç?xar?r.
        /// </summary>
        /// <param name="sessionId">?stifad?çinin session ID-si</param>
        /// <param name="carId">Ç?xar?lacaq avtomobilin id-si</param>
        Task RemoveAsync(string sessionId, int carId);

        /// <summary>
        /// Verilmi? session-a aid müqayis? siyah?s?n? tamamil? t?mizl?yir.
        /// </summary>
        Task ClearAsync(string sessionId);

        /// <summary>
        /// Verilmi? avtomobilin h?min session-un müqayis? siyah?s?nda olub-olmad???n? yoxlay?r.
        /// </summary>
        Task<bool> IsInListAsync(string sessionId, int carId);

        // ??? ADMIN metodlar? ???????????????????????????????????????????????????

        /// <summary>
        /// Köhn?lmi? (mü?yy?n tarixd?n ?vv?l yarad?lm??) müqayis? qeydl?rini bazadan silir.
        /// Planla?d?r?lm?? i? (background job) t?r?find?n ça??r?la bil?r.
        /// </summary>
        /// <param name="olderThan">Bu tarixd?n ?vv?lki qeydl?r silin?c?k</param>
        Task CleanupExpiredAsync(DateTime olderThan);
    }
}
