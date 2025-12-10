using System.ComponentModel.DataAnnotations;

namespace LopTopWebApi.Contracts
{
    public sealed class LoginRequest
    {
        [Required]
        [StringLength(100)]
        public string Login { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Password { get; set; } = string.Empty;
    }
}
