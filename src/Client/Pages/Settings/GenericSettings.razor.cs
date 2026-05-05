using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FinancialFreedom.Shared;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace FinancialFreedom.Client.Pages.Settings;

public partial class GenericSettings
{
    [Inject] private HttpClient Http { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    private ProjectionAssumptionsDto? _model;
    private bool _loading = true;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _model = await Http.GetFromJsonAsync<ProjectionAssumptionsDto>("api/projection-assumptions");
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task Save()
    {
        if (_model is null)
            return;
        try
        {
            var res = await Http.PutAsJsonAsync("api/projection-assumptions", _model);
            res.EnsureSuccessStatusCode();
            Snackbar.Add("Saved.", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add(ex.Message, Severity.Error);
        }
    }
}
