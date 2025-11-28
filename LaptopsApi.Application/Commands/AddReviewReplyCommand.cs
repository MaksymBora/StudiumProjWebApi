using MediatR;

namespace LaptopsApi.Application.Commands
{
    public sealed class AddReviewReplyCommand : IRequest<Guid>
    {
        public Guid ProductId { get; set; }
        public Guid ParentReviewId { get; set; }
        public Guid UserId { get; set; }
        public string? Comment { get; set; }
    }
}
