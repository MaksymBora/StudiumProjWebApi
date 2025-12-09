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

            var specsIds = products
                .Where(p => p.SpecsId.HasValue)
                .Select(p => p.SpecsId!.Value)
                .Distinct()
                .ToList();

            if (specsIds.Count > 0)
            {
                var specsEntities = await _context.Specs
                    .AsNoTracking()
                    .Where(s => specsIds.Contains(s.SpecsId))
                    .ToListAsync(cancellationToken);

                var specsDict = specsEntities.ToDictionary(
                    s => s.SpecsId,
                    s => new SpecsDto
                    {
                        SpecsId = s.SpecsId,
                        Processor = s.Processor,
                        RamGb = s.RamGb,
                        RamType = s.RamType,
                        StorageGb = s.StorageGb,
                        StorageType = s.StorageType,
                        StorageInterface = s.StorageInterface,
                        Gpu = s.Gpu,
                        GpuType = s.GpuType,
                        BatteryCapacityWh = s.BatteryCapacityWh,
                        BatteryLifeHours = s.BatteryLifeHours,
                        CoolingSystem = s.CoolingSystem,
                        DisplayResolution = s.DisplayResolution,
                        DisplayRefreshRate = s.DisplayRefreshRate,
                        PortsDescription = s.PortsDescription,
                        WeightKg = s.WeightKg,
                        Dimensions = s.Dimensions,
                        OperatingSystem = s.OperatingSystem,
                        WarrantyMonths = s.WarrantyMonths,
                        AdditionalFeatures = s.AdditionalFeatures
                    });

                foreach (var p in products)
                {
                    if (p.SpecsId.HasValue && specsDict.TryGetValue(p.SpecsId.Value, out var dto))
                    {
                        p.Specs = dto;
                    }
                }
            }

            if (products.Count == 0)
                return products;

            var productIds = products.Select(p => p.ProductId).ToList();

            var reviewRows = await (
                from r in _context.Reviews.AsNoTracking()
                join u in _context.Users.AsNoTracking()
                    on r.UserId equals u.UserId
                where !r.IsDeleted
                select new
                {
                    r.ReviewId,
                    r.ProductId,
                    r.ParentReviewId,
                    r.ReviewDate,
                    r.Comment,
                    r.UserId,
                    r.Rating,
                    UserName = u.Username
                }).ToListAsync(cancellationToken);

            var dtoById = reviewRows.ToDictionary(
                x => x.ReviewId,
                x => new ProductReviewDto
                {
                    ReviewId = x.ReviewId,
                    UserId = x.UserId,
                    UserName = x.UserName,
                    Comment = x.Comment,
                    CreatedAtUtc = x.ReviewDate,
                    ParentId = x.ParentReviewId,
                    Rating = x.Rating,
                    Children = new List<ProductReviewDto>()
                });

            var childrenLookup = reviewRows
                .Where(x => x.ParentReviewId != null)
                .GroupBy(x => x.ParentReviewId!.Value)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => dtoById[x.ReviewId])
                          .OrderBy(d => d.CreatedAtUtc)
                          .ToList()
                );

            List<ProductReviewDto> BuildTreeForProduct(Guid productId)
            {
                var roots = reviewRows
                    .Where(x => x.ProductId == productId && x.ParentReviewId == null)
                    .OrderBy(x => x.ReviewDate)
                    .Select(x => dtoById[x.ReviewId])
                    .ToList();

                void AttachChildren(ProductReviewDto node)
                {
                    if (childrenLookup.TryGetValue(node.ReviewId, out var kids))
                    {
                        node.Children = kids;

                        foreach (var child in kids)
                        {
                            AttachChildren(child); 
                        }
                    }
                }

                foreach (var root in roots)
                {
                    AttachChildren(root);
                }

                return roots;
            }

            foreach (var p in products)
            {
                p.Reviews = BuildTreeForProduct(p.ProductId);
            }

            return products;
        }
    }
}
