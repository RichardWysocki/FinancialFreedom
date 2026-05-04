using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FinancialFreedom.Shared;
using Microsoft.AspNetCore.Components;

namespace FinancialFreedom.Client.Pages;

public partial class Timeline
{
    [Inject] private HttpClient Http { get; set; } = default!;

    private List<TimelinePointDto>? _points;

    protected override async Task OnInitializedAsync()
    {
        _points = await Http.GetFromJsonAsync<List<TimelinePointDto>>("api/timeline");
    }
}
