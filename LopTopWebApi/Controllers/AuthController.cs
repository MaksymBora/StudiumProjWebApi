using LaptopsApi.Application.Commands;
using LaptopsApi.Infrastructure.Services;
using LopTopWebApi.Contracts;
using LopTopWebApi.Domain.Entities;
using LopTopWebApi.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;

namespace LopTopWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IUserRepository _users;
        private readonly IPasswordHasher _hasher;
        private readonly ITokenService _tokens;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IMediator mediator,
            IUserRepository users,
            IPasswordHasher hasher,
            ITokenService tokens,
            ILogger<AuthController> logger)
        {
            _mediator = mediator;
            _users = users;
            _hasher = hasher;
            _tokens = tokens;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest body, CancellationToken ct)
        {
            _logger.LogInformation("Registering new user {Username}", body.Username);

            var id = await _mediator.Send(new RegisterUserCommand
            {
                Username = body.Username,
                Email = body.Email,
                Password = body.Password,
                FirstName = body.FirstName,
                LastName = body.LastName
            }, ct);

            return Created(string.Empty, new { userId = id });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest body, CancellationToken ct)
        {
            var token = await _mediator.Send(new LoginCommand
            {
                Login = body.Login,
                Password = body.Password
            }, ct);

            return Ok(new { accessToken = token, tokenType = "Bearer" });
        }

        [HttpPost("firebase-login")]
        [Authorize(AuthenticationSchemes = "Firebase")]
        public async Task<IActionResult> FirebaseLogin(CancellationToken ct)
        {
            var firebaseUid =
                User.FindFirstValue("user_id") ??
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("sub");

            var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized("Firebase token does not contain email.");

            var normalizedEmail = email.Trim().ToLowerInvariant();
            var existingUser = await _users.FindByUsernameOrEmailAsync(normalizedEmail, ct);
            var user = existingUser;

            if (user is null)
            {
                var firstName = User.FindFirstValue("given_name") ?? string.Empty;
                var lastName = User.FindFirstValue("family_name") ?? string.Empty;
                var preferredName = User.FindFirstValue("name") ?? string.Empty;

                var username = await BuildUniqueUsernameAsync(normalizedEmail, preferredName, ct);
                var generatedPasswordHash = _hasher.Hash(Guid.NewGuid().ToString("N"));

                user = LopTopWebApi.Domain.Entities.User.Create(
                    firstName: firstName,
                    lastName: lastName,
                    username: username,
                    email: normalizedEmail,
                    passwordHash: generatedPasswordHash);

                await _users.AddAsync(user, ct);
                await _users.SaveChangesAsync(ct);
            }

            var accessToken = _tokens.CreateAccessToken(user.UserId, user.Username, user.Email);
            return Ok(new
            {
                accessToken,
                tokenType = "Bearer",
                provider = "Firebase",
                firebaseUid
            });
        }

        private async Task<string> BuildUniqueUsernameAsync(string email, string preferredName, CancellationToken ct)
        {
            var emailPrefix = email.Split('@')[0];
            var baseCandidate = string.IsNullOrWhiteSpace(preferredName) ? emailPrefix : preferredName;

            var normalizedBase = NormalizeUsername(baseCandidate);
            if (normalizedBase.Length < 3)
                normalizedBase = "user";

            if (normalizedBase.Length > 60)
                normalizedBase = normalizedBase[..60];

            var candidate = normalizedBase;
            var suffix = 1;

            while (await _users.UsernameExistsAsync(candidate, ct))
            {
                var suffixText = suffix.ToString();
                var maxBaseLength = Math.Max(3, 60 - suffixText.Length);
                candidate = normalizedBase[..Math.Min(normalizedBase.Length, maxBaseLength)] + suffixText;
                suffix++;
            }

            return candidate;
        }

        private static string NormalizeUsername(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var buffer = new StringBuilder(value.Length);
            foreach (var ch in value.Trim().ToLowerInvariant())
            {
                if (char.IsLetterOrDigit(ch) || ch == '_' || ch == '.' || ch == '-')
                    buffer.Append(ch);
            }

            return buffer.ToString();
        }
    }
}
