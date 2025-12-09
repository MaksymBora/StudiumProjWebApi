using MediatR;

namespace LaptopsApi.Application.Commands
{
    public sealed class AddProductCommand : IRequest<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Brand { get; set; } = string.Empty;
        public decimal ScreenSize { get; set; }
        public string? Description { get; set; }
        public Guid UserId { get; set; }

        public AddSpecsCommand? Specs { get; set; }
    }
}

