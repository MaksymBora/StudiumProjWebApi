using MediatR;

namespace LaptopsApi.Application.Commands
{
    public sealed class LoginCommand : IRequest<string>
    {
        public string Login { get; init; } = "";
        public string Password { get; init; } = "";
    }
}
