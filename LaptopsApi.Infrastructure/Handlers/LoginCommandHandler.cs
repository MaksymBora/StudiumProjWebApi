using LaptopsApi.Application.Commands;
using LaptopsApi.Infrastructure.Services;
using LopTopWebApi.Domain.Interfaces;
using MediatR;

namespace LaptopsApi.Infrastructure.Handlers
{
    public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, string>
    {
        private readonly IUserRepository _repo;
        private readonly IPasswordHasher _hasher;
        private readonly ITokenService _tokens;

        public LoginCommandHandler(IUserRepository repo, IPasswordHasher hasher, ITokenService tokens)
        {
            _repo = repo;
            _hasher = hasher;
            _tokens = tokens;
        }

        public async Task<string> Handle(LoginCommand req, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req.Login) || string.IsNullOrWhiteSpace(req.Password))
                throw new ArgumentException("Login and password are required.");

            var user = await _repo.FindByUsernameOrEmailAsync(req.Login.Trim(), ct)
                       ?? throw new UnauthorizedAccessException("Invalid credentials.");

            if (!_hasher.Verify(req.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid credentials.");

            // если у тебя есть роль — подставь из user
            var access = _tokens.CreateAccessToken(user.UserId, user.Username, user.Email, role: null);
            return access;
        }
    }
}
