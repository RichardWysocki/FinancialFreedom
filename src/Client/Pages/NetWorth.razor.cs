using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FinancialFreedom.Shared;
using Microsoft.AspNetCore.Components;

namespace FinancialFreedom.Client.Pages;

public partial class NetWorth
{
    [Inject] private HttpClient Http { get; set; } = default!;

    private NetWorthSummaryDto? _nw;

    protected override async Task OnInitializedAsync()
    {
        _nw = await Http.GetFromJsonAsync<NetWorthSummaryDto>("api/net-worth/summary");
    }
}
