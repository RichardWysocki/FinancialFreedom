using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FinancialFreedom.Shared;
using Microsoft.AspNetCore.Components;

namespace FinancialFreedom.Client.Pages.FinancialHealth;

public partial class SavingsByAge
{
    [Inject] private HttpClient Http { get; set; } = default!;

    private SavingsByAgeResultDto? _result;
    private string? _error;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _result = await Http.GetFromJsonAsync<SavingsByAgeResultDto>("api/projections/savings-by-age");
        }
        catch (Exception ex)
        {
            _error = ex.Message;
        }
    }
}
