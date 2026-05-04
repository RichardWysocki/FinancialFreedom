using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FinancialFreedom.Shared;
using Microsoft.AspNetCore.Components;

namespace FinancialFreedom.Client.Pages;

public partial class PageVisitLog
{
    [Inject] private HttpClient Http { get; set; } = default!;

    private List<PageVisitLogEntryDto>? _rows;
    private string? _error;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _rows = await Http.GetFromJsonAsync<List<PageVisitLogEntryDto>>("api/PageVisitLog?take=200");
        }
        catch
        {
            _error = "Could not load visit log. Sign in and ensure the database has the PageVisitLogs table (run migrations).";
        }
    }
}
