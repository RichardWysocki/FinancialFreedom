using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using FinancialFreedom.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace FinancialFreedom.Client.Pages;

public partial class FutureSalaryCalculator
{
    [Inject] private HttpClient Http { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;

    private readonly FutureSalaryProjectionRequest _model = new()
    {
        CurrentAge = 35,
        RetirementAge = 65,
        CurrentSalary = 100_000m,
        AnnualIncreasePercent = 3m,
    };

    private readonly List<FutureSalaryProjectionRow> _rows = [];
    private string? _formError;
    private int _startYear;
    private int _endYear;
    private bool _loading;

    private async Task BuildTable()
    {
        _formError = null;
        _rows.Clear();
        _loading = true;
        try
        {
            var body = new FutureSalaryProjectionRequest
            {
                CurrentAge = _model.CurrentAge,
                RetirementAge = _model.RetirementAge,
                CurrentSalary = _model.CurrentSalary,
                AnnualIncreasePercent = _model.AnnualIncreasePercent,
                CalendarStartYear = DateTime.Today.Year,
            };

            var response = await Http.PostAsJsonAsync("api/FutureSalary/project", body);
            if (response.IsSuccessStatusCode)
            {
                var payload = await response.Content.ReadFromJsonAsync<FutureSalaryProjectionResponse>();
                if (payload?.Rows is { Count: > 0 })
                {
                    _startYear = payload.CalendarStartYear;
                    _endYear = payload.CalendarEndYear;
                    _rows.AddRange(payload.Rows);
                }
                else
                {
                    _formError = "Unexpected empty response from the server.";
                }
            }
            else
            {
                var err = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                _formError = err?.Message ?? $"Request failed ({(int)response.StatusCode}).";
            }
        }
        catch (HttpRequestException ex)
        {
            _formError = $"Could not reach the server: {ex.Message}";
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task ExportCsv()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Age,Calendar year,Projected salary");
        foreach (var row in _rows)
        {
            sb.Append(row.Age.ToString(CultureInfo.InvariantCulture)).Append(',');
            sb.Append(row.CalendarYear.ToString(CultureInfo.InvariantCulture)).Append(',');
            sb.AppendLine(row.Salary.ToString("0.##", CultureInfo.InvariantCulture));
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        var b64 = Convert.ToBase64String(bytes);
        await JS.InvokeVoidAsync("ff.download", "future-salary-projection.csv", b64);
    }

    private static string FormatMoney(decimal value) =>
        value.ToString("C0", CultureInfo.CurrentCulture);
}
