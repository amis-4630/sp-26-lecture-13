using System.Security.Claims;
using Buckeye.Lending.Api.Extensions;
using FluentAssertions;

namespace Buckeye.Lending.Api.Tests.Unit;

public class ClaimsPrincipalExtensionsTests
{
    [Fact]
    public void GetUserId_ReturnsCorrectId_WhenClaimPresent()
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "42") };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        var result = principal.GetUserId();

        result.Should().Be(42);
    }

    [Fact]
    public void GetUserId_ThrowsInvalidOperationException_WhenClaimMissing()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        var act = () => principal.GetUserId();

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*NameIdentifier*");
    }
}
