using LaptopsApi.Application.Common.DTOs;
using MediatR;

namespace LaptopsApi.Application.Queries
{
    public class GetProductsQuery : MediatR.IRequest<PagedResult<ProductDto>>
    {
        public string? Brand { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? MinRamGb { get; set; }
        public string? Sort {  get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 9;
    }
}