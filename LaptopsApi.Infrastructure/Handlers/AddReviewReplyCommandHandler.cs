using LaptopsApi.Application.Commands;
using LopTopWebApi.Domain.Interfaces;
using MediatR;

namespace LaptopsApi.Infrastructure.Handlers
{
    public sealed class AddReviewReplyCommandHandler : IRequestHandler<AddReviewReplyCommand, Guid>
    {
        private readonly IReviewRepository _repo;

        public AddReviewReplyCommandHandler(IReviewRepository repo)
        {
            _repo = repo;
        }

        public async Task<Guid> Handle(AddReviewReplyCommand req, CancellationToken ct)
        {
            if (req.ProductId == Guid.Empty)
                throw new ArgumentException("ProductId required");
            if (req.ParentReviewId == Guid.Empty)
                throw new ArgumentException("ParentReviewId required");
            if (req.UserId == Guid.Empty)
                throw new ArgumentException("UserId required");

            var parent = await _repo.GetByIdAsync(req.ParentReviewId, ct)
                         ?? throw new ArgumentException("Parent review not found.");

            if (parent.ProductId != null && parent.ProductId != req.ProductId)
                throw new InvalidOperationException("Parent review does not belong to this product.");

            var id = await _repo.AddReplyAsync(req.ParentReviewId, req.UserId, req.Comment, ct);
            return id;
        }
    }
}
