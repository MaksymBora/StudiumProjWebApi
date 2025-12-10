using System.Text.RegularExpressions;
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

        private static readonly Regex UsernameRegex =
            new Regex(@"^[a-zA-Z0-9_.-]+$", RegexOptions.Compiled);

        private static readonly Regex NameRegex =
            new Regex(@"^\p{L}+$", RegexOptions.Compiled);

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

            var rawUsername = req.Username.Trim();
            var normalizedUsername = rawUsername.ToLowerInvariant();

            var email = req.Email.Trim().ToLowerInvariant();

            if (normalizedUsername.Length is < 3 or > 60)
                throw new ArgumentException("Username must be between 3 and 60 characters.");

            if (!UsernameRegex.IsMatch(rawUsername))
                throw new ArgumentException("Username contains invalid characters.");

            if (email.Length > 100)
                throw new ArgumentException("Email is too long.");

            if (!string.IsNullOrWhiteSpace(req.FirstName) &&
                !NameRegex.IsMatch(req.FirstName.Trim()))
                throw new ArgumentException("First name contains invalid characters.");

            if (!string.IsNullOrWhiteSpace(req.LastName) &&
                !NameRegex.IsMatch(req.LastName.Trim()))
                throw new ArgumentException("Last name contains invalid characters.");

            if (await _repo.EmailExistsAsync(email, ct))
                throw new InvalidOperationException("Email already in use.");

            if (await _repo.UsernameExistsAsync(normalizedUsername, ct))
                throw new InvalidOperationException("Username already in use.");

            var hash = _hasher.Hash(req.Password);

            var user = User.Create(
                firstName: req.FirstName ?? "",
                lastName: req.LastName ?? "",
                username: normalizedUsername, 
                email: email,
                passwordHash: hash
            );

            await _repo.AddAsync(user, ct);
            await _repo.SaveChangesAsync(ct);

            return user.UserId;
        }
    }
}
