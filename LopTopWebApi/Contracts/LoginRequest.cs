using System.ComponentModel.DataAnnotations;

namespace LopTopWebApi.Contracts
{
    public sealed class LoginRequest
    {
        [Required] public string Login { get; set; } = "";  
        [Required] public string Password { get; set; } = "";
    }
}
