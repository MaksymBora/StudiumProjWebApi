using MediatR;

namespace LaptopsApi.Application.Queries
{
    public sealed class GetProductRatingQuery : IRequest<double?>
    {
        public Guid ProductId { get; init; }
    }
}
