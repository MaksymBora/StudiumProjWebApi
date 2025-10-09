namespace LopTopWebApi.Domain.Entities
{
    public class Product
    {
        public Guid ProductId { get; private set; } = Guid.NewGuid();
        public string Name { get; private set; } = string.Empty;
        public decimal Price { get; private set; }
        public string Brand { get; private set; } = string.Empty;
        public decimal ScreenSize { get; private set; }
        public string? Description { get; private set; }

        public Guid? AddedByUserId { get; private set; }
        public DateTime AddedDate { get; private set; } = DateTime.UtcNow;

        // FK на Specs (1:1)
        public Guid? SpecsId { get; private set; }
        public virtual Specs? Specs { get; private set; }

        // Новый столбец под маппинг create_date
        public DateTime CreateDate { get; private set; }

        // === Factory ===
        public static Product Create(
            string name,
            decimal price,
            string brand,
            decimal screenSize,
            string? description = null,
            Guid? addedByUserId = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name is required");
            if (price <= 0)
                throw new ArgumentException("Price must be greater than 0");
            if (screenSize <= 0)
                throw new ArgumentException("Screen size must be greater than 0");

            return new Product
            {
                Name = name.Trim(),
                Price = price,
                Brand = brand.Trim(),
                ScreenSize = screenSize,
                Description = description,
                AddedByUserId = addedByUserId
                // CreateDate SQl can make on default
            };
        }

        // === Business ===
        public void UpdatePrice(decimal newPrice)
        {
            if (newPrice <= 0)
                throw new ArgumentException("New price must be greater than 0");
            Price = newPrice;
        }
    }
}
