using Car_Project.Models;

namespace Car_Project.Services.Abstractions
{
    public interface IProductService
    {
        // ?? PUBLIC ????????????????????????????????????????????????????????????

        /// <summary>
        /// Aktiv m?hsullar? filtrasiya v? s?hif?l?m? il? qaytar?r.
        /// </summary>
        Task<(IList<Product> Items, int TotalCount)> GetFilteredAsync(
            int page, int pageSize = 12,
            string? categorySlug = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string? sortBy = null,
            string? searchQuery = null);

        /// <summary>
        /// Id-y? gör? t?k m?hsulu bütün ??kill?ri il? qaytar?r. Tap?lmasa null.
        /// </summary>
        Task<Product?> GetByIdAsync(int id);

        /// <summary>
        /// Slug-a gör? t?k m?hsulu qaytar?r. Tap?lmasa null.
        /// </summary>
        Task<Product?> GetBySlugAsync(string slug);

        /// <summary>
        /// Eyni kateqoriyadan ?laq?li m?hsullar? qaytar?r.
        /// </summary>
        Task<IList<Product>> GetRelatedAsync(int categoryId, int excludeProductId, int count = 4);

        /// <summary>
        /// Seçilmi? (IsFeatured = true) m?hsullar? qaytar?r.
        /// </summary>
        Task<IList<Product>> GetFeaturedAsync(int count = 8);

        /// <summary>
        /// Bütün aktiv kateqoriyalar? qaytar?r.
        /// </summary>
        Task<IList<ProductCategory>> GetCategoriesAsync();

        // ?? ADMIN ?????????????????????????????????????????????????????????????

        Task<IList<Product>> GetAllAdminAsync();
        Task<(IList<Product> Items, int TotalCount)> GetPagedAdminAsync(int page, int pageSize = 10);
        Task<Product> CreateAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int id);
        Task ToggleActiveAsync(int id);
        Task<bool> ExistsAsync(int id);

        Task<ProductCategory> CreateCategoryAsync(ProductCategory category);
        Task UpdateCategoryAsync(ProductCategory category);
        Task DeleteCategoryAsync(int id);
    }

    public interface ICartService
    {
        /// <summary>Session-a aid bütün s?b?t elementl?rini m?hsul m?lumatlar? il? qaytar?r.</summary>
        Task<IList<CartItem>> GetCartAsync(string sessionId);

        /// <summary>S?b?td?ki ümumi m?hsul say?n? qaytar?r (nav-bar sayac?).</summary>
        Task<int> GetCartCountAsync(string sessionId);

        /// <summary>S?b?t? m?hsul ?lav? edir. Art?q varsa miqdar?n? art?r?r.</summary>
        Task AddToCartAsync(string sessionId, int productId, int quantity = 1);

        /// <summary>S?b?td?ki elementin miqdar?n? yenil?yir.</summary>
        Task UpdateQuantityAsync(string sessionId, int productId, int quantity);

        /// <summary>S?b?td?n m?hsulu ç?xar?r.</summary>
        Task RemoveFromCartAsync(string sessionId, int productId);

        /// <summary>Séssiyaya aid bütün s?b?ti t?mizl?yir.</summary>
        Task ClearCartAsync(string sessionId);

        /// <summary>Köhn?lmi? (vaxt? keçmi?) s?b?t qeydl?rini silir.</summary>
        Task CleanupExpiredAsync(DateTime olderThan);
    }
}
