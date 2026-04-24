using System.Net.Http.Json;
using FinancialFreedom.Shared;

namespace FinancialFreedom.UnitTests;

public class WeatherForecastApiTests : IClassFixture<FinancialFreedomWebAppFactory>
{
    private readonly HttpClient _client;

    public WeatherForecastApiTests(FinancialFreedomWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task WeatherForecast_returns_five_rows()
    {
        var forecasts = await _client.GetFromJsonAsync<WeatherForecast[]>("/WeatherForecast");
        Assert.NotNull(forecasts);
        Assert.Equal(5, forecasts.Length);
    }
}
