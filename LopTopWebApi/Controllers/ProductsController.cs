using Microsoft.AspNetCore.Mvc;
using LaptopsApi.Application.Queries;
using LaptopsApi.Application.Common.DTOs;
using LopTopWebApi.Domain.Interfaces;
using AutoMapper;
using MediatR;
using LaptopsApi.Application.Commands;
using LopTopWebApi.Contracts;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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
                // Пользователь уже голосовал – 409 вполне логично
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
    }
}