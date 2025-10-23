using MediatR;

namespace LaptopsApi.Application.Commands
{
    /// <summary>
    /// Command for adding a product rating (root review).
    /// </summary>
    public sealed class AddProductRatingCommand : IRequest<Guid>
    {
        public Guid ProductId { get; init; }
        public Guid UserId { get; init; }
        public int Rating { get; init; }
        public string? Comment { get; init; }
    }
}
