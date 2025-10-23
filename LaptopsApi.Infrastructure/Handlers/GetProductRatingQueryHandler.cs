using LaptopsApi.Application.Queries;
using LaptopsApi.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LaptopsApi.Infrastructure.Handlers
{
    public sealed class GetProductRatingQueryHandler : IRequestHandler<GetProductRatingQuery, double?>
    {
        private readonly AppDbContext _ctx;

        public GetProductRatingQueryHandler(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<double?> Handle(GetProductRatingQuery req, CancellationToken ct)
        {
            if (req.ProductId == Guid.Empty)
                throw new ArgumentException("ProductId required");

            var ratings = _ctx.Reviews
                .AsNoTracking()
                .Where(r => r.ProductId == req.ProductId
                            && r.ParentReviewId == null
                            && r.Rating != null
                            && !r.IsDeleted)
                .Select(r => (double)r.Rating!);

            if (!await ratings.AnyAsync(ct))
                return null; 

            return await ratings.AverageAsync(ct);
        }
    }
}
