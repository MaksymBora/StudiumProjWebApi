using System.ComponentModel.DataAnnotations;

namespace LopTopWebApi.Contracts
{
    public sealed class RegisterRequest
    {
        [Required, MinLength(3)]
        public string Username { get; set; } = "";

        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required, MinLength(6)]
        public string Password { get; set; } = "";

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}
