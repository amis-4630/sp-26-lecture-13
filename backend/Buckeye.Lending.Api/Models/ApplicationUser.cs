using Microsoft.AspNetCore.Identity;

namespace Buckeye.Lending.Api.Models;

/// <summary>
/// Application user — extends IdentityUser with int keys to match existing entities.
/// </summary>
public class ApplicationUser : IdentityUser<int>
{
    public string FullName { get; set; } = string.Empty;
}
