using Car_Project.Data;
using Car_Project.Models;
using Car_Project.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Car_Project.Services
{
    public class BlogService : IBlogService
    {
        private readonly ApplicationDbContext _context;

        public BlogService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(IList<BlogPost> Items, int TotalCount)> GetPublishedAsync(
            int page, int pageSize = 9,
            string? categorySlug = null,
            string? tagName = null,
            string? searchQuery = null)
        {
            if (page < 1) page = 1;

            var query = _context.BlogPosts
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.PostTags).ThenInclude(pt => pt.BlogTag)
                .Where(p => p.Status == BlogStatus.Published)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(categorySlug))
                query = query.Where(p => p.Category.Slug == categorySlug);

            if (!string.IsNullOrWhiteSpace(tagName))
                query = query.Where(p => p.PostTags.Any(pt => pt.BlogTag.Name == tagName));

            if (!string.IsNullOrWhiteSpace(searchQuery))
                query = query.Where(p =>
                    p.Title.Contains(searchQuery) ||
                    (p.Summary != null && p.Summary.Contains(searchQuery)));

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(p => p.PublishedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<BlogPost?> GetBySlugAsync(string slug)
        {
            var post = await _context.BlogPosts
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.PostTags).ThenInclude(pt => pt.BlogTag)
                .FirstOrDefaultAsync(p =>
                    p.Slug == slug && p.Status == BlogStatus.Published);

            if (post == null) return null;

            // Filtered Include AsNoTracking ile dogru islemeyebilir,
            // ona gore commentleri ayri sorgu ile cekivik
            var comments = await _context.BlogComments
                .AsNoTracking()
                .Where(c => c.BlogPostId == post.Id && c.IsApproved)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();

            // Navigation property-ye manual assign edirik
            post.Comments = comments;

            return post;
        }

        public async Task<IList<BlogPost>> GetPopularAsync(int count = 5)
        {
            return await _context.BlogPosts
                .AsNoTracking()
                .Include(p => p.Category)
                .Where(p => p.Status == BlogStatus.Published)
                .OrderByDescending(p => p.ViewCount)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IList<BlogPost>> GetRecentAsync(int count = 5)
        {
            return await _context.BlogPosts
                .AsNoTracking()
                .Include(p => p.Category)
                .Where(p => p.Status == BlogStatus.Published)
                .OrderByDescending(p => p.PublishedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IList<BlogPost>> GetRelatedAsync(int categoryId, int excludePostId, int count = 3)
        {
            return await _context.BlogPosts
                .AsNoTracking()
                .Include(p => p.Category)
                .Where(p =>
                    p.CategoryId == categoryId &&
                    p.Id != excludePostId &&
                    p.Status == BlogStatus.Published)
                .OrderByDescending(p => p.PublishedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<(BlogPost? Prev, BlogPost? Next)> GetAdjacentPostsAsync(int postId)
        {
            var current = await _context.BlogPosts
                .AsNoTracking()
                .Where(p => p.Id == postId && p.Status == BlogStatus.Published)
                .Select(p => new { p.Id, p.PublishedAt })
                .FirstOrDefaultAsync();

            if (current == null) return (null, null);

            var prev = await _context.BlogPosts
                .AsNoTracking()
                .Include(p => p.Category)
                .Where(p => p.Status == BlogStatus.Published && p.PublishedAt < current.PublishedAt)
                .OrderByDescending(p => p.PublishedAt)
                .FirstOrDefaultAsync();

            var next = await _context.BlogPosts
                .AsNoTracking()
                .Include(p => p.Category)
                .Where(p => p.Status == BlogStatus.Published && p.PublishedAt > current.PublishedAt)
                .OrderBy(p => p.PublishedAt)
                .FirstOrDefaultAsync();

            return (prev, next);
        }

        public async Task<IList<BlogCategory>> GetCategoriesAsync()
        {
            return await _context.BlogCategories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IList<BlogTag>> GetTagsAsync()
        {
            return await _context.BlogTags
                .AsNoTracking()
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task IncrementViewCountAsync(int postId)
        {
            var post = await _context.BlogPosts.FindAsync(postId);
            if (post == null) return;
            post.ViewCount++;
            await _context.SaveChangesAsync();
        }

        // ADMIN

        public async Task<IList<BlogPost>> GetAllAdminAsync()
        {
            return await _context.BlogPosts
                .AsNoTracking()
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
        }

        public async Task<BlogPost?> GetByIdAsync(int id)
        {
            return await _context.BlogPosts
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.PostTags).ThenInclude(pt => pt.BlogTag)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<BlogPost> CreateAsync(BlogPost post, IList<int> tagIds)
        {
            if (post == null) throw new ArgumentNullException(nameof(post));

            post.CreatedDate = DateTime.UtcNow;

            if (string.IsNullOrWhiteSpace(post.Slug))
                post.Slug = GenerateSlug(post.Title);

            await _context.BlogPosts.AddAsync(post);
            await _context.SaveChangesAsync();

            await SyncTagsAsync(post.Id, tagIds);
            return post;
        }

        public async Task UpdateAsync(BlogPost post, IList<int> tagIds)
        {
            if (post == null) throw new ArgumentNullException(nameof(post));

            var existing = await _context.BlogPosts.FindAsync(post.Id)
                ?? throw new KeyNotFoundException($"Id={post.Id} post not found.");

            existing.Title           = post.Title;
            existing.Slug            = string.IsNullOrWhiteSpace(post.Slug) ? GenerateSlug(post.Title) : post.Slug;
            existing.Summary         = post.Summary;
            existing.Content         = post.Content;
            existing.ThumbnailUrl    = post.ThumbnailUrl;
            existing.AuthorName      = post.AuthorName;
            existing.AuthorAvatarUrl = post.AuthorAvatarUrl;
            existing.CategoryId      = post.CategoryId;
            existing.Status          = post.Status;

            await _context.SaveChangesAsync();
            await SyncTagsAsync(post.Id, tagIds);
        }

        public async Task PublishAsync(int id)
        {
            var post = await _context.BlogPosts.FindAsync(id)
                ?? throw new KeyNotFoundException($"Id={id} post not found.");
            post.Status      = BlogStatus.Published;
            post.PublishedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task ArchiveAsync(int id)
        {
            var post = await _context.BlogPosts.FindAsync(id)
                ?? throw new KeyNotFoundException($"Id={id} post not found.");
            post.Status = BlogStatus.Archived;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var post = await _context.BlogPosts.FindAsync(id)
                ?? throw new KeyNotFoundException($"Id={id} post not found.");
            _context.BlogPosts.Remove(post);
            await _context.SaveChangesAsync();
        }

        // CATEGORY

        public async Task<BlogCategory> CreateCategoryAsync(BlogCategory category)
        {
            if (category == null) throw new ArgumentNullException(nameof(category));
            if (string.IsNullOrWhiteSpace(category.Slug))
                category.Slug = GenerateSlug(category.Name);

            category.CreatedDate = DateTime.UtcNow;
            await _context.BlogCategories.AddAsync(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task UpdateCategoryAsync(BlogCategory category)
        {
            var existing = await _context.BlogCategories.FindAsync(category.Id)
                ?? throw new KeyNotFoundException($"Id={category.Id} category not found.");

            existing.Name = category.Name;
            existing.Slug = string.IsNullOrWhiteSpace(category.Slug) ? GenerateSlug(category.Name) : category.Slug;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCategoryAsync(int id)
        {
            var category = await _context.BlogCategories
                .Include(c => c.Posts)
                .FirstOrDefaultAsync(c => c.Id == id)
                ?? throw new KeyNotFoundException($"Id={id} category not found.");

            if (category.Posts.Any())
                throw new InvalidOperationException($"Category has {category.Posts.Count} posts. Delete posts first.");

            _context.BlogCategories.Remove(category);
            await _context.SaveChangesAsync();
        }

        // TAG

        public async Task<BlogTag> CreateTagAsync(BlogTag tag)
        {
            if (tag == null) throw new ArgumentNullException(nameof(tag));
            tag.CreatedDate = DateTime.UtcNow;
            await _context.BlogTags.AddAsync(tag);
            await _context.SaveChangesAsync();
            return tag;
        }

        public async Task DeleteTagAsync(int id)
        {
            var tag = await _context.BlogTags.FindAsync(id)
                ?? throw new KeyNotFoundException($"Id={id} tag not found.");
            _context.BlogTags.Remove(tag);
            await _context.SaveChangesAsync();
        }

        // COMMENTS

        public async Task<BlogComment> AddCommentAsync(BlogComment comment)
        {
            if (comment == null) throw new ArgumentNullException(nameof(comment));
            comment.IsApproved  = true;
            comment.CreatedDate = DateTime.UtcNow;
            await _context.BlogComments.AddAsync(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task ApproveCommentAsync(int commentId)
        {
            var comment = await _context.BlogComments.FindAsync(commentId)
                ?? throw new KeyNotFoundException($"Id={commentId} comment not found.");
            comment.IsApproved = true;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCommentAsync(int commentId)
        {
            var comment = await _context.BlogComments.FindAsync(commentId)
                ?? throw new KeyNotFoundException($"Id={commentId} comment not found.");
            _context.BlogComments.Remove(comment);
            await _context.SaveChangesAsync();
        }

        // HELPERS

        private async Task SyncTagsAsync(int postId, IList<int> tagIds)
        {
            var existing = await _context.BlogPostTags
                .Where(pt => pt.BlogPostId == postId)
                .ToListAsync();
            _context.BlogPostTags.RemoveRange(existing);

            if (tagIds?.Count > 0)
            {
                var newMappings = tagIds.Distinct().Select(tid => new BlogPostTag
                {
                    BlogPostId = postId,
                    BlogTagId  = tid
                });
                await _context.BlogPostTags.AddRangeAsync(newMappings);
            }
            await _context.SaveChangesAsync();
        }

        private static string GenerateSlug(string title) =>
            title.ToLowerInvariant()
                 .Replace(" ", "-")
                 .Replace("e", "e").Replace("o", "o").Replace("u", "u")
                 .Replace("i", "i").Replace("g", "g").Replace("s", "s")
                 .Replace("c", "c")
                 .Trim('-');
    }
}
