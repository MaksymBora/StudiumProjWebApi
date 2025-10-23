using LaptopsApi.Application.Commands;
using LopTopWebApi.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LopTopWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IMediator mediator, ILogger<AuthController> logger)
        {
            _mediator = mediator;
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
    }
}
