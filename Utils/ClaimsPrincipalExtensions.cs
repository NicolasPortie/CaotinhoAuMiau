using System.Security.Claims;

namespace CaotinhoAuMiau.Utils
{
    public static class ClaimsPrincipalExtensions
    {
        public static string ObterIdUsuario(this ClaimsPrincipal user)
        {
            return user?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        }

        public static string ObterValorClaim(this ClaimsPrincipal user, string tipoClaim)
        {
            return user?.FindFirst(tipoClaim)?.Value ?? string.Empty;
        }
    }
}
