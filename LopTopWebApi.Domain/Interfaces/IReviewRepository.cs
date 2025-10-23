namespace LopTopWebApi.Domain.Interfaces
{
    public interface IReviewRepository
    {
        Task<bool> HasUserRatedProductAsync(Guid productId, Guid userId, CancellationToken ct);
        Task<Guid> AddRootReviewAsync(Guid productId, Guid userId, int rating, string? comment, CancellationToken ct);
    }
}
