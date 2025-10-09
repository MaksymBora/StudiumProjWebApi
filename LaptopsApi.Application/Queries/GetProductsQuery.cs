using LaptopsApi.Application.Common.DTOs;

namespace LaptopsApi.Application.Queries
{
    public class GetProductsQuery : MediatR.IRequest<IEnumerable<ProductDto>>
    {
        public string? Brand { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? MinRamGb { get; set; }
    }
}