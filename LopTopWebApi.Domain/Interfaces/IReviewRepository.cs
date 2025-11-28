using LopTopWebApi.Domain.Entities;

namespace LopTopWebApi.Domain.Interfaces
{
    public interface IReviewRepository
    {
        Task<bool> HasUserRatedProductAsync(Guid productId, Guid userId, CancellationToken ct);
        Task<Guid> AddRootReviewAsync(Guid productId, Guid userId, int rating, string? comment, CancellationToken ct);
        Task<Review?> GetByIdAsync(Guid reviewId, CancellationToken ct);
        Task<Guid> AddReplyAsync(Guid parentReviewId, Guid userId, string? comment, CancellationToken ct);
    }
}
