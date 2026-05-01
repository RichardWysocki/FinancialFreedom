using System.Net;
using System.Net.Http.Json;
using FinancialFreedom.Shared;

namespace FinancialFreedom.UnitTests;

public class PageVisitLogApiTests : IClassFixture<FinancialFreedomWebAppFactory>
{
    private readonly HttpClient _client;

    public PageVisitLogApiTests(FinancialFreedomWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Post_log_is_allowed_without_auth_and_returns_no_content()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/PageVisitLog",
            new PageVisitLogRequest { PagePath = "/unit-test", QueryString = "x=1" });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Get_recent_requires_authentication()
    {
        var response = await _client.GetAsync("/api/PageVisitLog");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
