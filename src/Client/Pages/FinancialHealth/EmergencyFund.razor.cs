using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FinancialFreedom.Shared;
using Microsoft.AspNetCore.Components;

namespace FinancialFreedom.Client.Pages.FinancialHealth;

public partial class EmergencyFund
{
    [Inject] private HttpClient Http { get; set; } = default!;

    private DashboardDto? _dashboard;

    protected override async Task OnInitializedAsync()
    {
        _dashboard = await Http.GetFromJsonAsync<DashboardDto>("api/dashboard");
    }
}
