namespace LaptopsApi.Application.Common.DTOs
{
    public sealed class ProductReviewDto
    {
        public Guid ReviewId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? Comment { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public int? Rating { get; set; }
        public Guid? ParentId { get; set; }
        public List<ProductReviewDto> Children { get; set; } = new();
    }
}
