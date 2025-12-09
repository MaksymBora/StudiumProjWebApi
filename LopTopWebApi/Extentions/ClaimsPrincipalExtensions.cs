using System.Security.Claims;

namespace LopTopWebApi.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var id =
                user.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                user.FindFirst(ClaimTypes.Name)?.Value ??       
                user.FindFirst("sub")?.Value;                   

            if (string.IsNullOrWhiteSpace(id))
                throw new InvalidOperationException("User id claim not found in token.");

            if (!Guid.TryParse(id, out var guid))
                throw new InvalidOperationException("Invalid user id format in token.");

            return guid;
        }
    }
}
