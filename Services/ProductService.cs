using Car_Project.Data;
using Car_Project.Models;
using Car_Project.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Car_Project.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;

        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ?? PUBLIC ????????????????????????????????????????????????????????????

        public async Task<(IList<Product> Items, int TotalCount)> GetFilteredAsync(
            int page, int pageSize = 12,
            string? categorySlug = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string? sortBy = null,
            string? searchQuery = null)
        {
            if (page < 1) page = 1;

            var query = _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Images.Where(i => i.IsMain))
                .Where(p => p.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(categorySlug))
                query = query.Where(p => p.Category.Slug == categorySlug);

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            if (!string.IsNullOrWhiteSpace(searchQuery))
                query = query.Where(p => p.Name.Contains(searchQuery));

            query = sortBy switch
            {
                "price-asc"  => query.OrderBy(p => p.Price),
                "price-desc" => query.OrderByDescending(p => p.Price),
                "newest"     => query.OrderByDescending(p => p.CreatedDate),
                _            => query.OrderByDescending(p => p.IsFeatured)
                                     .ThenByDescending(p => p.CreatedDate)
            };

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Images.OrderBy(i => i.Order))
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        }

        public async Task<Product?> GetBySlugAsync(string slug)
        {
            return await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Images.OrderBy(i => i.Order))
                .FirstOrDefaultAsync(p => p.Slug == slug && p.IsActive);
        }

        public async Task<IList<Product>> GetRelatedAsync(
            int categoryId, int excludeProductId, int count = 4)
        {
            return await _context.Products
                .AsNoTracking()
                .Include(p => p.Images.Where(i => i.IsMain))
                .Where(p =>
                    p.CategoryId == categoryId &&
                    p.Id != excludeProductId &&
                    p.IsActive)
                .OrderByDescending(p => p.CreatedDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IList<Product>> GetFeaturedAsync(int count = 8)
        {
            return await _context.Products
                .AsNoTracking()
                .Include(p => p.Images.Where(i => i.IsMain))
                .Where(p => p.IsFeatured && p.IsActive)
                .OrderByDescending(p => p.CreatedDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IList<ProductCategory>> GetCategoriesAsync()
        {
            return await _context.ProductCategories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        // ?? ADMIN ?????????????????????????????????????????????????????????????

        public async Task<IList<Product>> GetAllAdminAsync()
        {
            return await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
        }

        public async Task<(IList<Product> Items, int TotalCount)> GetPagedAdminAsync(
            int page, int pageSize = 10)
        {
            if (page < 1) page = 1;

            var query = _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedDate);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<Product> CreateAsync(Product product)
        {
            if (product == null) throw new ArgumentNullException(nameof(product));

            if (string.IsNullOrWhiteSpace(product.Slug))
                product.Slug = GenerateSlug(product.Name);

            product.CreatedDate = DateTime.UtcNow;
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task UpdateAsync(Product product)
        {
            if (product == null) throw new ArgumentNullException(nameof(product));

            var existing = await _context.Products.FindAsync(product.Id)
                ?? throw new KeyNotFoundException($"Id={product.Id} olan məhsul tapılmadı.");

            existing.Name        = product.Name;
            existing.Slug        = string.IsNullOrWhiteSpace(product.Slug)
                                     ? GenerateSlug(product.Name)
                                     : product.Slug;
            existing.Description = product.Description;
            existing.Price       = product.Price;
            existing.OldPrice    = product.OldPrice;
            existing.Stock       = product.Stock;
            existing.ThumbnailUrl = product.ThumbnailUrl;
            existing.Badge       = product.Badge;
            existing.BadgeColor  = product.BadgeColor;
            existing.IsActive    = product.IsActive;
            existing.IsFeatured  = product.IsFeatured;
            existing.CategoryId  = product.CategoryId;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id)
                ?? throw new KeyNotFoundException($"Id={id} olan məhsul tapılmadı.");

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }

        public async Task ToggleActiveAsync(int id)
        {
            var product = await _context.Products.FindAsync(id)
                ?? throw new KeyNotFoundException($"Id={id} olan məhsul tapılmadı.");

            product.IsActive = !product.IsActive;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id) =>
            await _context.Products.AnyAsync(p => p.Id == id);

        public async Task<ProductCategory> CreateCategoryAsync(ProductCategory category)
        {
            if (string.IsNullOrWhiteSpace(category.Slug))
                category.Slug = GenerateSlug(category.Name);
            category.CreatedDate = DateTime.UtcNow;
            await _context.ProductCategories.AddAsync(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task UpdateCategoryAsync(ProductCategory category)
        {
            var existing = await _context.ProductCategories.FindAsync(category.Id)
                ?? throw new KeyNotFoundException($"Id={category.Id} olan kateqoriya tapılmadı.");
            existing.Name    = category.Name;
            existing.Slug    = category.Slug;
            existing.IconUrl = category.IconUrl;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCategoryAsync(int id)
        {
            var category = await _context.ProductCategories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id)
                ?? throw new KeyNotFoundException($"Id={id} olan kateqoriya tapılmadı.");

            if (category.Products.Any())
                throw new InvalidOperationException(
                    $"Bu kateqoriyada {category.Products.Count} məhsul var.");

            _context.ProductCategories.Remove(category);
            await _context.SaveChangesAsync();
        }

        private static string GenerateSlug(string name) =>
            name.ToLowerInvariant().Replace(" ", "-").Trim('-');
    }
}
