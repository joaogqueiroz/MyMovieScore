using System.Security.Claims;

namespace MyMovieScore.Api.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        // The signed-in user's id, from the token's "sub" claim (mapped to NameIdentifier by
        // JwtBearer). Null for a token without it, such as one issued before the claim existed.
        public static int? GetUserId(this ClaimsPrincipal user) =>
            int.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value, out var id) ? id : null;
    }
}
