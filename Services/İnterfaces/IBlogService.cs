using Car_Project.Models;
using Car_Project.ViewModels.Blog;

namespace Car_Project.Services.Abstractions
{
    public interface IBlogService
    {
        // PUBLIC

        Task<(IList<BlogPost> Items, int TotalCount)> GetPublishedAsync(
            int page, int pageSize = 9,
            string? categorySlug = null,
            string? tagName = null,
            string? searchQuery = null);

        Task<BlogPost?> GetBySlugAsync(string slug);

        Task<IList<BlogPost>> GetPopularAsync(int count = 5);

        Task<IList<BlogPost>> GetRecentAsync(int count = 5);

        Task<IList<BlogPost>> GetRelatedAsync(int categoryId, int excludePostId, int count = 3);

        Task<(BlogPost? Prev, BlogPost? Next)> GetAdjacentPostsAsync(int postId);

        Task<IList<BlogCategory>> GetCategoriesAsync();

        Task<IList<BlogTag>> GetTagsAsync();

        Task IncrementViewCountAsync(int postId);

        // ADMIN

        Task<IList<BlogPost>> GetAllAdminAsync();

        Task<BlogPost?> GetByIdAsync(int id);

        Task<BlogPost> CreateAsync(BlogPost post, IList<int> tagIds);

        Task UpdateAsync(BlogPost post, IList<int> tagIds);

        Task PublishAsync(int id);

        Task ArchiveAsync(int id);

        Task DeleteAsync(int id);

        // CATEGORY

        Task<BlogCategory> CreateCategoryAsync(BlogCategory category);
        Task UpdateCategoryAsync(BlogCategory category);
        Task DeleteCategoryAsync(int id);

        // TAG

        Task<BlogTag> CreateTagAsync(BlogTag tag);
        Task DeleteTagAsync(int id);

        // COMMENTS

        Task<BlogComment> AddCommentAsync(BlogComment comment);
        Task ApproveCommentAsync(int commentId);
        Task DeleteCommentAsync(int commentId);
    }
}
