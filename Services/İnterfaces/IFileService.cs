namespace Car_Project.Services.Abstractions
{
    /// <summary>
    /// Fayl yükl?m?, silm? v? yoxlama ?m?liyyatlar? üçün servis interfeysi.
    /// wwwroot/uploads qovlu?una ??kil yükl?m?k üçün istifad? olunur.
    /// </summary>
    public interface IFileService
    {
        /// <summary>
        /// T?k bir fayl? yükl?yir v? saxlan?lan nisbi yolu qaytar?r.
        /// </summary>
        /// <param name="file">Yükl?n?c?k fayl (IFormFile)</param>
        /// <param name="folderName">wwwroot daxilind? h?d?f qovluq (m?s: "uploads/cars")</param>
        /// <returns>Saxlan?lan fayl?n nisbi yolu (m?s: "/uploads/cars/abc123.jpg")</returns>
        Task<string> UploadAsync(IFormFile file, string folderName);

        /// <summary>
        /// Bir neç? fayl? paralel yükl?yir.
        /// </summary>
        /// <param name="files">Yükl?n?c?k fayllar?n siyah?s?</param>
        /// <param name="folderName">wwwroot daxilind? h?d?f qovluq</param>
        /// <returns>Saxlan?lan fayllar?n nisbi yollar?n?n siyah?s?</returns>
        Task<IList<string>> UploadManyAsync(IList<IFormFile> files, string folderName);

        /// <summary>
        /// Mövcud fayl? sirir.
        /// </summary>
        /// <param name="filePath">Silin?c?k fayl?n nisbi yolu (m?s: "/uploads/cars/abc123.jpg")</param>
        /// <returns>Fayl tap?l?b silindis? true, tap?lmad?sa false</returns>
        bool Delete(string filePath);

        /// <summary>
        /// Mövcud fayl? yenisi il? ?v?z edir — köhn?ni silir, yenini yükl?yir.
        /// </summary>
        /// <param name="oldFilePath">Silin?c?k köhn? fayl?n nisbi yolu</param>
        /// <param name="newFile">Yükl?n?c?k yeni fayl</param>
        /// <param name="folderName">wwwroot daxilind? h?d?f qovluq</param>
        /// <returns>Yeni fayl?n nisbi yolu</returns>
        Task<string> ReplaceAsync(string oldFilePath, IFormFile newFile, string folderName);

        /// <summary>
        /// Verilmi? yolda fayl?n mövcud olub-olmad???n? yoxlay?r.
        /// </summary>
        /// <param name="filePath">Yoxlan?lacaq fayl?n nisbi yolu</param>
        bool Exists(string filePath);
    }
}
