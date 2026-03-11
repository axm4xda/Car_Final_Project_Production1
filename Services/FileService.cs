using Car_Project.Services.Abstractions;

namespace Car_Project.Services
{
    /// <summary>
    /// IFileService implementasiyas?.
    /// Fayllar? wwwroot/uploads/... alt?na saxlay?r, GUID ?sasl? ad verir.
    /// </summary>
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;

        // ?caz? veril?n ??kil geni?l?nm?l?ri (kiçik h?rfl?)
        private static readonly HashSet<string> _allowedExtensions =
            new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

        // Maksimum fayl ölçüsü: 5 MB
        private const long MaxFileSizeBytes = 5 * 1024 * 1024;

        public FileService(IWebHostEnvironment env)
        {
            _env = env;
        }

        /// <inheritdoc />
        public async Task<string> UploadAsync(IFormFile file, string folderName)
        {
            ValidateFile(file);

            var uploadPath = GetUploadPath(folderName);
            var uniqueFileName = GenerateUniqueFileName(file.FileName);
            var fullPath = Path.Combine(uploadPath, uniqueFileName);

            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            // Nisbi URL yolu qaytar?r: /uploads/cars/abc123.jpg
            return $"/{folderName.TrimStart('/')}/{uniqueFileName}";
        }

        /// <inheritdoc />
        public async Task<IList<string>> UploadManyAsync(IList<IFormFile> files, string folderName)
        {
            if (files == null || files.Count == 0)
                throw new ArgumentException("Fayl siyah?s? bo? ola bilm?z.", nameof(files));

            var uploadTasks = files.Select(f => UploadAsync(f, folderName));
            var results = await Task.WhenAll(uploadTasks);
            return results.ToList();
        }

        /// <inheritdoc />
        public bool Delete(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return false;

            var fullPath = GetFullPath(filePath);

            if (!File.Exists(fullPath))
                return false;

            File.Delete(fullPath);
            return true;
        }

        /// <inheritdoc />
        public async Task<string> ReplaceAsync(string oldFilePath, IFormFile newFile, string folderName)
        {
            if (newFile == null)
                throw new ArgumentNullException(nameof(newFile));

            // Köhn? fayl? sil (tap?lmasa da davam et)
            Delete(oldFilePath);

            return await UploadAsync(newFile, folderName);
        }

        /// <inheritdoc />
        public bool Exists(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return false;

            return File.Exists(GetFullPath(filePath));
        }

        // ?? Köm?kçi metodlar ?????????????????????????????????????????????????

        /// <summary>wwwroot alt?ndak? tam fiziki yolu qaytar?r.</summary>
        private string GetFullPath(string relativePath)
        {
            // "/uploads/cars/abc.jpg" ? "C:\...\wwwroot\uploads\cars\abc.jpg"
            var normalized = relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(_env.WebRootPath, normalized);
        }

        /// <summary>Qovlu?un mövcud olmad??? halda yarad?r v? tam yolunu qaytar?r.</summary>
        private string GetUploadPath(string folderName)
        {
            var path = Path.Combine(
                _env.WebRootPath,
                folderName.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path;
        }

        /// <summary>GUID + orijinal geni?l?nm? il? unikal fayl ad? yarad?r.</summary>
        private static string GenerateUniqueFileName(string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
            return $"{Guid.NewGuid():N}{extension}";
        }

        /// <summary>Fayl?n geni?l?nm?sini v? ölçüsünü yoxlay?r.</summary>
        private static void ValidateFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Fayl bo? ola bilm?z.");

            if (file.Length > MaxFileSizeBytes)
                throw new InvalidOperationException(
                    $"Fayl ölçüsü {MaxFileSizeBytes / 1024 / 1024} MB limitini a??r.");

            var extension = Path.GetExtension(file.FileName);
            if (!_allowedExtensions.Contains(extension))
                throw new InvalidOperationException(
                    $"'{extension}' geni?l?nm?si d?st?kl?nmir. ?caz? veril?nl?r: {string.Join(", ", _allowedExtensions)}");
        }
    }
}
