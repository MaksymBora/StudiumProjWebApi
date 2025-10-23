using LaptopsApi.Application.Commands;
using LopTopWebApi.Domain.Interfaces;
using MediatR;

namespace LaptopsApi.Infrastructure.Handlers
{
    public sealed class AddProductRatingCommandHandler : IRequestHandler<AddProductRatingCommand, Guid>
    {
        private readonly IReviewRepository _repo;
        public AddProductRatingCommandHandler(IReviewRepository repo) => _repo = repo;

        public async Task<Guid> Handle(AddProductRatingCommand req, CancellationToken ct)
        {
            if (req.ProductId == Guid.Empty) throw new ArgumentException("ProductId required");
            if (req.UserId == Guid.Empty) throw new ArgumentException("UserId required");
            if (req.Rating < 1 || req.Rating > 5) throw new ArgumentException("Rating must be between 1 and 5");

            if (await _repo.HasUserRatedProductAsync(req.ProductId, req.UserId, ct))
                throw new InvalidOperationException("User has already rated this product.");

            return await _repo.AddRootReviewAsync(req.ProductId, req.UserId, req.Rating, req.Comment, ct);
        }
    }
}
