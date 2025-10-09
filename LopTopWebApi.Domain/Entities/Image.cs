namespace LopTopWebApi.Domain.Entities
{
    public class Image
    {
        public Guid ImageId { get; private set; } = Guid.NewGuid();
        public Guid ProductId { get; private set; }
        public string Url { get; private set; } = string.Empty;
        public bool IsMain { get; private set; } = false;
        public DateTime UploadDate { get; private set; } = DateTime.UtcNow;

        // Navigation property
        public virtual Product Product { get; private set; } = null!;

        // Factory method
        public static Image Create(Guid productId, string url, bool isMain = false)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("Image URL is required");

            return new Image
            {
                ProductId = productId,
                Url = url.Trim(),
                IsMain = isMain
            };
        }
    }
}