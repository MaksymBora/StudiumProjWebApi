using Microsoft.AspNetCore.Mvc;
using LaptopsApi.Application.Queries;
using LopTopWebApi.Domain.Interfaces;
using AutoMapper;
using MediatR;
using LaptopsApi.Application.Commands;
using LopTopWebApi.Contracts;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using LopTopWebApi.Extensions;

namespace loptopwebapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductRepository productRepository,IMediator mediator,IMapper mapper,ILogger<ProductsController> logger)
        {
            _productRepository = productRepository;
            _mediator = mediator;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Get all laptops without filters
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var products = await _mediator.Send(new GetProductsQuery(), ct);
            return Ok(products);
        }

        /// <summary>
        /// Get product by id
        /// </summary>
        [HttpGet("{productId:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid productId, CancellationToken ct)
        {
            var query = new GetProductsQuery
            {
             
            };

            var all = await _mediator.Send(query, ct);
            var product = all.FirstOrDefault(p => p.ProductId == productId);

            if (product is null)
                return NotFound();

            return Ok(product);
        }


        /// <summary>
        /// Get laptops with optional filters
        /// </summary>
        /// <param name="brand">Filter by brand (e.g., Dell)</param>
        /// <param name="minPrice">Minimum price</param>
        /// <param name="maxPrice">Maximum price</param>
        /// <param name="minRamGb">Minimum RAM in GB</param>
        [HttpGet("filter")]
        public async Task<IActionResult> GetFiltered(CancellationToken ct, [FromQuery] string? brand = null,[FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null, [FromQuery] int? minRamGb = null)
        {
            var query = new GetProductsQuery()
            {
                Brand = brand,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                MinRamGb = minRamGb
            };

            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }

        /// <summary>
        /// Add rating
        /// </summary>
        [HttpPost("{productId:guid}/rating")]
        [Authorize] 
        public async Task<IActionResult> AddRating([FromRoute] Guid productId,[FromBody, Required] AddRatingRequest body, CancellationToken ct)
        {
            _logger.LogInformation("Add rating: productId={ProductId},  rating={Rating}",
                productId, body.Rating);

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdStr)) return Forbid();

            var userId = Guid.Parse(userIdStr);

            try
            {
                var reviewId = await _mediator.Send(new AddProductRatingCommand
                {
                    ProductId = productId,
                    UserId = userId,
                    Rating = body.Rating,
                    Comment = body.Comment
                }, ct);

                return CreatedAtAction(nameof(GetRating), new { productId }, new { reviewId });
            }
            catch (AlreadyRatedException ex)
            {
                _logger.LogWarning(ex, "User already rated product {ProductId}", productId);
                return Conflict(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error when adding rating");
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Business rule violation when adding rating");
                
                return Conflict(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when adding rating");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error = "Unexpected error occurred." });
            }
        }

        /// <summary>
        /// Get avg rate
        /// </summary>
        [HttpGet("{productId:guid}/rating")]
        public async Task<IActionResult> GetRating([FromRoute] Guid productId, CancellationToken ct)
        {
            var avg = await _mediator.Send(new GetProductRatingQuery { ProductId = productId }, ct);

            var result = avg.HasValue ? Math.Round(avg.Value, 1) : (double?)null;
            return Ok(new { productId, averageRating = result });
        }

        /// <summary>
        /// Add reply to existing review (child comment).
        /// </summary>
        [Authorize]
        [HttpPost("{productId:guid}/reviews/{parentReviewId:guid}/reply")]
        public async Task<IActionResult> AddReply(Guid productId, Guid parentReviewId, [FromBody] AddReplyRequest body, CancellationToken ct)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;

            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized(new { message = "Invalid user id in token." });

            var cmd = new AddReviewReplyCommand
            {
                ProductId = productId,
                ParentReviewId = parentReviewId,
                UserId = userId,
                Comment = body.Comment
            };

            var replyId = await _mediator.Send(cmd, ct);

            return Created(string.Empty, new { reviewId = replyId });
        }

        /// <summary>
        /// Create new product
        /// </summary>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AddProductRequest body, CancellationToken ct)
        {
            var userId = User.GetUserId();

            var cmd = new AddProductCommand
            {
                Name = body.Name,
                Price = body.Price,
                Brand = body.Brand,
                ScreenSize = body.ScreenSize,
                Description = body.Description,
                UserId = userId,
                Specs = body.Specs is null
                    ? null
                    : new AddSpecsCommand
                    {
                        Processor = body.Specs.Processor,
                        RamGb = body.Specs.RamGb,
                        RamType = body.Specs.RamType,
                        StorageGb = body.Specs.StorageGb,
                        StorageType = body.Specs.StorageType,
                        StorageInterface = body.Specs.StorageInterface,
                        Gpu = body.Specs.Gpu,
                        GpuType = body.Specs.GpuType,
                        BatteryCapacityWh = body.Specs.BatteryCapacityWh,
                        BatteryLifeHours = body.Specs.BatteryLifeHours,
                        CoolingSystem = body.Specs.CoolingSystem,
                        DisplayResolution = body.Specs.DisplayResolution,
                        DisplayRefreshRate = body.Specs.DisplayRefreshRate,
                        PortsDescription = body.Specs.PortsDescription,
                        WeightKg = body.Specs.WeightKg,
                        Dimensions = body.Specs.Dimensions,
                        OperatingSystem = body.Specs.OperatingSystem,
                        WarrantyMonths = body.Specs.WarrantyMonths,
                        AdditionalFeatures = body.Specs.AdditionalFeatures
                    }
            };

            var id = await _mediator.Send(cmd, ct);

            return Created(string.Empty, new { productId = id });
        }
    }
}