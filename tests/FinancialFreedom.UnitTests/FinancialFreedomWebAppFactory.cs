using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace FinancialFreedom.UnitTests;

public class FinancialFreedomWebAppFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var inMemoryDbName = "Test_" + Guid.NewGuid().ToString("N");
        builder.UseEnvironment("Development");
        builder.UseSetting("ASPNETCORE_HTTPS_PORT", "7005");
        // Host settings override appsettings.Development.json (which disables in-memory).
        builder.UseSetting("Database:UseInMemory", "true");
        builder.UseSetting("Database:InMemoryDatabaseName", inMemoryDbName);
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:UseInMemory"] = "true",
                ["Database:InMemoryDatabaseName"] = inMemoryDbName,
            });
        });
    }
}
