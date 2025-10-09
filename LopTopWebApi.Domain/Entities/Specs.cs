namespace LopTopWebApi.Domain.Entities
{
    public class Specs
    {
        public Guid SpecsId { get; private set; } = Guid.NewGuid();
        public int? RamGb { get; private set; }
        public virtual Product Product { get; private set; } = null!;

        // Factory method
        public static Specs Create(Guid productId, int? ramGb = null)
        {
            return new Specs
            {
                RamGb = ramGb
            };
        }
    }
}