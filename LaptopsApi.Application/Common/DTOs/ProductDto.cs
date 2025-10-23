namespace LaptopsApi.Application.Common.DTOs
{
    public class ProductDto
    {
        public Guid ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Brand { get; set; } = string.Empty;
        public decimal ScreenSize { get; set; }
        public string? Description { get; set; }
        public Guid? AddedByUserId { get; set; }
        public DateTime AddedDate { get; set; }
        public Guid? SpecsId { get; set; }

    }
}