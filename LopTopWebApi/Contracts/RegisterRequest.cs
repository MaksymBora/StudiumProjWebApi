using System.ComponentModel.DataAnnotations;

namespace LopTopWebApi.Contracts
{
    public sealed class RegisterRequest
    {
        [Required]
        [StringLength(60, MinimumLength = 3,
            ErrorMessage = "Username must be between 3 and 60 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9_.-]+$",
            ErrorMessage = "Username can contain only letters, digits, '.', '_' and '-'.")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(100,
            ErrorMessage = "Email must be at most 100 characters.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6,
            ErrorMessage = "Password must be between 6 and 100 characters.")]
        public string Password { get; set; } = string.Empty;

        [StringLength(60)]
        [RegularExpression(@"^\p{L}+$",
            ErrorMessage = "First name can contain only letters.")]
        public string? FirstName { get; set; }

        [StringLength(60)]
        [RegularExpression(@"^\p{L}+$",
            ErrorMessage = "Last name can contain only letters.")]
        public string? LastName { get; set; }
    }
}
