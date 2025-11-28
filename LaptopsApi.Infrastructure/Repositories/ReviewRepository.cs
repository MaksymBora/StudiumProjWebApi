using LaptopsApi.Infrastructure.Data;
using LopTopWebApi.Domain.Entities;
using LopTopWebApi.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LaptopsApi.Infrastructure.Repositories
{
    public sealed class ReviewRepository : IReviewRepository
    {
        private readonly AppDbContext _ctx;
        public ReviewRepository(AppDbContext ctx) => _ctx = ctx;

        public async Task<bool> HasUserRatedProductAsync(Guid productId, Guid userId, CancellationToken ct)
        {
            return await _ctx.Reviews
                .AsNoTracking()
                .AnyAsync(r => r.ProductId == productId
                            && r.UserId == userId
                            && r.ParentReviewId == null
                            && !r.IsDeleted, ct);
        }

        public async Task<Guid> AddRootReviewAsync(Guid productId, Guid userId, int rating, string? comment, CancellationToken ct)
        {
            var review = Review.CreateRootReview(productId, userId, rating, comment);
            _ctx.Reviews.Add(review);
            await _ctx.SaveChangesAsync(ct);
            return review.ReviewId;
        }

        public async Task<Review?> GetByIdAsync(Guid reviewId, CancellationToken ct)
        {
            return await _ctx.Reviews
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.ReviewId == reviewId, ct);
        }

        public async Task<Guid> AddReplyAsync(Guid parentReviewId, Guid userId, string? comment, CancellationToken ct)
        {
            var reply = Review.CreateReply(parentReviewId, userId, comment);

            _ctx.Reviews.Add(reply);
            await _ctx.SaveChangesAsync(ct);

            return reply.ReviewId;
        }
    }
}
