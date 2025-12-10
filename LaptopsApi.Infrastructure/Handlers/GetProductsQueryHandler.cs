using Microsoft.EntityFrameworkCore;
using LaptopsApi.Infrastructure.Data;
using LaptopsApi.Application.Common.DTOs;
using LaptopsApi.Application.Queries;
using MediatR;

namespace LaptopsApi.Infrastructure.Handlers
{
    public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PagedResult<ProductDto>>
    {
        private readonly AppDbContext _context;

        public GetProductsQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            var filteredProductsQuery = ApplyFilters(_context.Products, request);

            var totalItems = await filteredProductsQuery.CountAsync(cancellationToken);

            var page = request.PageNumber <= 0 ? 1 : request.PageNumber;
            var pageSize = request.PageSize <= 0 ? 12 : request.PageSize;
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var productQuery = BuildBaseProductDtoQuery(filteredProductsQuery);

            productQuery = ApplySorting(productQuery, request.Sort);

            productQuery = ApplyPaging(productQuery, page, pageSize);

            var products = await productQuery.ToListAsync(cancellationToken);

            await LoadSpecsForProductsAsync(products, cancellationToken);

            await LoadReviewsForProductsAsync(products, cancellationToken);

            return new PagedResult<ProductDto>
            {
                Items = products,
                PageNumber = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages
            };
        }


        /// <summary>
        /// Application of filters (brand, price, minimum RAM).
        /// </summary>
        private IQueryable<LopTopWebApi.Domain.Entities.Product> ApplyFilters(
            IQueryable<LopTopWebApi.Domain.Entities.Product> query,
            GetProductsQuery request)
        {
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

            return query;
        }

        /// <summary>
        /// Request that converts Product into ProductDto and calculates the AverageRating.
        /// </summary>
        private IQueryable<ProductDto> BuildBaseProductDtoQuery(
            IQueryable<LopTopWebApi.Domain.Entities.Product> productsQuery)
        {
            return
                from p in productsQuery.AsNoTracking()
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
        }

        /// <summary>
        /// Applies sorting based on the sort string.
        /// </summary>
        private IQueryable<ProductDto> ApplySorting(
            IQueryable<ProductDto> query,
            string? sort)
        {
            if (string.IsNullOrWhiteSpace(sort))
                return query.OrderBy(p => p.Name);

            switch (sort.Trim().ToLowerInvariant())
            {
                case "rating_desc":
                    return query.OrderByDescending(p => p.AverageRating);

                case "rating_asc":
                    return query.OrderBy(p => p.AverageRating);

                case "price_desc":
                    return query.OrderByDescending(p => p.Price);

                case "price_asc":
                    return query.OrderBy(p => p.Price);

                default:
                    return query.OrderBy(p => p.Name);
            }
        }

        /// <summary>
        /// Pagination.
        /// </summary>
        private IQueryable<ProductDto> ApplyPaging(
            IQueryable<ProductDto> query,
            int page,
            int pageSize)
        {
            return query
                .Skip((page - 1) * pageSize)
                .Take(pageSize);
        }

        /// <summary>
        /// Loads the specs and maps them into ProductDto.Specs.
        /// </summary>
        private async Task LoadSpecsForProductsAsync(
            List<ProductDto> products,
            CancellationToken ct)
        {
            var specsIds = products
                .Where(p => p.SpecsId.HasValue)
                .Select(p => p.SpecsId!.Value)
                .Distinct()
                .ToList();

            if (specsIds.Count == 0)
                return;

            var specsEntities = await _context.Specs
                .AsNoTracking()
                .Where(s => specsIds.Contains(s.SpecsId))
                .ToListAsync(ct);

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
                if (p.SpecsId.HasValue &&
                    specsDict.TryGetValue(p.SpecsId.Value, out var dto))
                {
                    p.Specs = dto;
                }
            }
        }

        /// <summary>
        /// Loads the reviews, builds the children tree, and assigns them to the products.
        /// </summary>
        private async Task LoadReviewsForProductsAsync(
            List<ProductDto> products,
            CancellationToken ct)
        {
            if (products.Count == 0)
                return;

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
                }).ToListAsync(ct);

            if (reviewRows.Count == 0)
                return;

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
        }
    }
}
