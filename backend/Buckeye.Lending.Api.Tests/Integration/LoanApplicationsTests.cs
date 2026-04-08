using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;

namespace Buckeye.Lending.Api.Tests.Integration;

public class LoanApplicationsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public LoanApplicationsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> GetTokenAsync(string email, string password)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { email, password });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return body!.Token;
    }

    [Fact]
    public async Task GetLoanApplications_Returns401_WhenUnauthenticated()
    {
        var response = await _client.GetAsync("/api/loanapplications");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetLoan_ReturnsNotFound_WhenLoanBelongsToAnotherUser()
    {
        // Login as regular user
        var userToken = await GetTokenAsync("user@buckeye.edu", "UserPass123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

        // Loan ID 1 belongs to admin — should return 404 for regular user
        var response = await _client.GetAsync("/api/loanapplications/1");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateLoan_Returns403_ForNonAdmin()
    {
        // Login as regular user
        var userToken = await GetTokenAsync("user@buckeye.edu", "UserPass123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

        // PUT is admin-only
        var response = await _client.PutAsJsonAsync("/api/loanapplications/5", new
        {
            applicantName = "Test",
            loanAmount = 1000,
            annualIncome = 50000,
            applicantId = 1,
            loanTypeId = 1
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private record LoginResponse(string Token, DateTime ExpiresAt, int UserId, string Email, string Role);
}
