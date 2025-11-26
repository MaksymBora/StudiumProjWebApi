using Microsoft.EntityFrameworkCore;
using LaptopsApi.Infrastructure.Data;
using LaptopsApi.Application.Common.DTOs;
using LaptopsApi.Application.Queries;
using MediatR;

namespace LaptopsApi.Infrastructure.Handlers
{
    public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, IEnumerable<ProductDto>>
    {
        private readonly AppDbContext _context;

        public GetProductsQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            IQueryable<LopTopWebApi.Domain.Entities.Product> query = _context.Products;

            if (!string.IsNullOrEmpty(request.Brand))
                query = query.Where(p => p.Brand == request.Brand);

            if (request.MinPrice.HasValue)
                query = query.Where(p => p.Price >= request.MinPrice.Value);

            if (request.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= request.MaxPrice.Value);

            if (request.MinRamGb.HasValue)
            {
                query =
                    from p in query
                    join s in _context.Specs on p.SpecsId equals s.SpecsId
                    where s.RamGb >= request.MinRamGb.Value
                    select p;
            }

            var resultQuery =
                from p in query.AsNoTracking()
                select new ProductDto
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Price = p.Price,
                    Brand = p.Brand,
                    ScreenSize = p.ScreenSize,
                    Description = p.Description,
                    AddedByUserId = p.AddedByUserId,
                    AddedDate = p.AddedDate,
                    SpecsId = p.SpecsId,

                    AverageRating =_context.Reviews
                    .Where(r => r.ProductId == p.ProductId && r.ParentReviewId == null && !r.IsDeleted && r.Rating.HasValue)
                    .Average(r => (double?)r.Rating) ?? 0.0
                };

            return await resultQuery.ToListAsync(cancellationToken);
        }
    }
}
