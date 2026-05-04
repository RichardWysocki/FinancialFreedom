using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FinancialFreedom.Shared;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace FinancialFreedom.Client.Pages.FinancialHealth;

public partial class IncomeReplacement
{
    [Inject] private HttpClient Http { get; set; } = default!;

    private IncomeReplacementResultDto? _data;
    private string? _error;

    private MudBlazor.Color TrafficColor =>
        _data?.TrafficLight switch
        {
            2 => MudBlazor.Color.Success,
            1 => MudBlazor.Color.Warning,
            _ => MudBlazor.Color.Error,
        };

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _data = await Http.GetFromJsonAsync<IncomeReplacementResultDto>("api/projections/income-replacement");
        }
        catch (Exception ex)
        {
            _error = ex.Message;
        }
    }
}
