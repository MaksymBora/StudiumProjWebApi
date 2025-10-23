using MediatR;

namespace LaptopsApi.Application.Commands
{
    public sealed class RegisterUserCommand : IRequest<Guid>
    {
        public string Username { get; init; } = "";
        public string Email { get; init; } = "";
        public string Password { get; init; } = "";
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
    }
}
