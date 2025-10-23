using LaptopsApi.Application.Commands;
using LaptopsApi.Infrastructure.Services;
using LopTopWebApi.Domain.Entities;
using LopTopWebApi.Domain.Interfaces;
using MediatR;

namespace LaptopsApi.Infrastructure.Handlers
{
    public sealed class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Guid>
    {
        private readonly IUserRepository _repo;
        private readonly IPasswordHasher _hasher;

        public RegisterUserCommandHandler(IUserRepository repo, IPasswordHasher hasher)
        {
            _repo = repo;
            _hasher = hasher;
        }

        public async Task<Guid> Handle(RegisterUserCommand req, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req.Username))
                throw new ArgumentException("Username required");
            if (string.IsNullOrWhiteSpace(req.Email))
                throw new ArgumentException("Email required");
            if (string.IsNullOrWhiteSpace(req.Password))
                throw new ArgumentException("Password required");

            var email = req.Email.Trim().ToLowerInvariant();
            var username = req.Username.Trim();

            if (await _repo.EmailExistsAsync(email, ct))
                throw new InvalidOperationException("Email already in use.");
            if (await _repo.UsernameExistsAsync(username, ct))
                throw new InvalidOperationException("Username already in use.");

            var hash = _hasher.Hash(req.Password);

            var user = User.Create(
                firstName: req.FirstName ?? "",
                lastName: req.LastName ?? "",
                username: username,
                email: email,
                passwordHash: hash
            );

            await _repo.AddAsync(user, ct);
            await _repo.SaveChangesAsync(ct);

            return user.UserId;
        }
    }
}
