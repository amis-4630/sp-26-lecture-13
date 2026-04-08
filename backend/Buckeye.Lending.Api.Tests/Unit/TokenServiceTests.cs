using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Buckeye.Lending.Api.Models;
using Buckeye.Lending.Api.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace Buckeye.Lending.Api.Tests.Unit;

public class TokenServiceTests
{
    private readonly TokenService _tokenService;

    public TokenServiceTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "TestSigningKeyThatIsAtLeast32CharactersLong!",
                ["Jwt:Issuer"] = "test-issuer",
                ["Jwt:Audience"] = "test-audience",
            })
            .Build();

        _tokenService = new TokenService(config);
    }

    [Fact]
    public void CreateToken_ReturnsTokenWithCorrectClaims()
    {
        var user = new ApplicationUser { Id = 7, Email = "test@buckeye.edu", FullName = "Test User" };
        var roles = new List<string> { "Admin" };

        var (token, expiresAt) = _tokenService.CreateToken(user, roles);

        token.Should().NotBeNullOrEmpty();
        expiresAt.Should().BeAfter(DateTime.UtcNow);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == "7");
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == "test@buckeye.edu");
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
        jwt.Issuer.Should().Be("test-issuer");
        jwt.Audiences.Should().Contain("test-audience");
    }
}
