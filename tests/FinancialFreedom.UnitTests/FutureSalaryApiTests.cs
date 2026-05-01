using System.Net;
using System.Net.Http.Json;
using FinancialFreedom.Shared;

namespace FinancialFreedom.UnitTests;

public class FutureSalaryApiTests : IClassFixture<FinancialFreedomWebAppFactory>
{
    private readonly HttpClient _client;

    public FutureSalaryApiTests(FinancialFreedomWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Project_returns_rows_for_valid_request()
    {
        var request = new FutureSalaryProjectionRequest
        {
            CurrentAge = 55,
            RetirementAge = 57,
            CurrentSalary = 80_000m,
            AnnualIncreasePercent = 2m,
            CalendarStartYear = 2030,
        };

        var response = await _client.PostAsJsonAsync("/api/FutureSalary/project", request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<FutureSalaryProjectionResponse>();
        Assert.NotNull(body);
        Assert.Equal(3, body!.Rows.Count);
        Assert.Equal(2030, body.CalendarStartYear);
        Assert.Equal(2032, body.CalendarEndYear);
    }

    [Fact]
    public async Task Project_returns_bad_request_when_salary_invalid()
    {
        var request = new FutureSalaryProjectionRequest
        {
            CurrentAge = 30,
            RetirementAge = 40,
            CurrentSalary = 0m,
            AnnualIncreasePercent = 1m,
            CalendarStartYear = 2026,
        };

        var response = await _client.PostAsJsonAsync("/api/FutureSalary/project", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var err = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.NotNull(err?.Message);
    }
}
