using Buckeye.Lending.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Buckeye.Lending.Api.Tests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"TestDb-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Provide JWT config so the app can start without user-secrets
        builder.ConfigureAppConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "IntegrationTestSigningKeyThatIsAtLeast32Chars!",
                ["Jwt:Issuer"] = "buckeye-lending",
                ["Jwt:Audience"] = "buckeye-lending-clients",
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove ALL existing LendingContext registrations
            var descriptors = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<LendingContext>)
                         || d.ServiceType == typeof(LendingContext))
                .ToList();
            foreach (var d in descriptors)
                services.Remove(d);

            // Add InMemory database with a unique name per factory instance
            services.AddDbContext<LendingContext>(options =>
                options.UseInMemoryDatabase(_dbName));
        });

        builder.UseEnvironment("Development");
    }
}
