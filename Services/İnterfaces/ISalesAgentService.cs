using Car_Project.Models;

namespace Car_Project.Services.Abstractions
{
    public interface ISalesAgentService
    {
        Task<IList<SalesAgent>> GetAllActiveAsync();
        Task<SalesAgent?> GetByIdAsync(int id);
        Task<SalesAgent> CreateAsync(SalesAgent agent);
        Task UpdateAsync(SalesAgent agent);
        Task DeleteAsync(int id);
        Task AddReviewAsync(SalesAgentReview review);
    }
}
