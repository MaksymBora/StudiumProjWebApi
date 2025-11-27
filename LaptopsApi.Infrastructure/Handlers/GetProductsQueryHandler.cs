using Microsoft.EntityFrameworkCore;
using LaptopsApi.Infrastructure.Data;
using LaptopsApi.Application.Common.DTOs;
using LaptopsApi.Application.Queries;
using MediatR;
using LaptopsApi.Infrastructure.Helpers;

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

            var productQuery = from p in query.AsNoTracking()
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

                                   AverageRating = _context.Reviews
                                   .Where(r => r.ProductId == p.ProductId &&
                                   r.ParentReviewId == null &&
                                   !r.IsDeleted &&
                                   r.Rating.HasValue)
                                   .Average(r => (double?)r.Rating) ?? 0.0
                               };

            var products = await productQuery.ToListAsync(cancellationToken);

            if (products.Count == 0)
                return products;

            var productIds = products.Select(p => p.ProductId).ToList();

            var rootRows = await (
            from r in _context.Reviews.AsNoTracking()
            join u in _context.Users.AsNoTracking()
                on r.UserId equals u.UserId
            where r.ProductId != null
                  && productIds.Contains(r.ProductId.Value)
                  && !r.IsDeleted
                  && r.ParentReviewId == null
            orderby r.ReviewDate
            select new
            {
                ProductId = r.ProductId!.Value,
                Review = new ProductReviewDto
                {
                    ReviewId = r.ReviewId,
                    UserId = r.UserId,
                    UserName = u.Username,
                    Comment = r.Comment,
                    CreatedAtUtc = r.ReviewDate,
                    ParentId = null,                 // корень
                    Children = new List<ProductReviewDto>()
                }
            }).ToListAsync(cancellationToken);

            var rootIds = rootRows.Select(x => x.Review.ReviewId).ToList();

            var childRows = await (
                from r in _context.Reviews.AsNoTracking()
                join root in _context.Reviews.AsNoTracking()
                    on r.ParentReviewId equals root.ReviewId
                join u in _context.Users.AsNoTracking()
                    on r.UserId equals u.UserId
                where root.ProductId != null
                      && productIds.Contains(root.ProductId.Value)
                      && !r.IsDeleted
                      && r.ParentReviewId != null
                orderby r.ReviewDate
                select new
                {
                    ProductId = root.ProductId!.Value,
                    Review = new ProductReviewDto
                    {
                        ReviewId = r.ReviewId,
                        UserId = r.UserId,
                        UserName = u.Username,
                        Comment = r.Comment,
                        CreatedAtUtc = r.ReviewDate,
                        ParentId = r.ParentReviewId,
                        Children = new List<ProductReviewDto>()
                    }
                }).ToListAsync(cancellationToken);

            var allRows = rootRows.Concat(childRows).ToList();

            var reviewsByProduct = allRows
                .GroupBy(x => x.ProductId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.Review).ToList()
                );


            foreach (var p in products)
            {
                if (reviewsByProduct.TryGetValue(p.ProductId, out var flatList))
                {
                    p.Reviews = BuildTree(flatList);
                }
                else
                {
                    p.Reviews = new List<ProductReviewDto>();
                }
            }

            return products;
        }

        private static List<ProductReviewDto> BuildTree(List<ProductReviewDto> flat)
        {
            var lookup = flat.ToDictionary(r => r.ReviewId);

            var roots = new List<ProductReviewDto>();

            foreach (var review in flat)
            {
                if (review.ParentId is null)
                {
                    roots.Add(review);
                }
                else
                {
                    if (lookup.TryGetValue(review.ParentId.Value, out var parent))
                    {
                        parent.Children.Add(review);
                    }
                }
            }

            return roots;
        }

    }
}
