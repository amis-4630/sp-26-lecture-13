using System.Security.Claims;

namespace Buckeye.Lending.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Extracts the user's integer ID from the NameIdentifier claim.
    /// </summary>
    public static int GetUserId(this ClaimsPrincipal principal)
    {
        var claim = principal.FindFirst(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("NameIdentifier claim is missing.");

        return int.Parse(claim.Value);
    }
}
