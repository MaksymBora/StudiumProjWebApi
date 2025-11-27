using LaptopsApi.Application.Queries;
using LaptopsApi.Infrastructure.Data;
using LaptopsApi.Infrastructure.Helpers;
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
                .RootRatingValuesForProduct(req.ProductId);

            if (!await ratings.AnyAsync(ct))
                return null;

            return await ratings.AverageAsync(ct);
        }
    }
}
