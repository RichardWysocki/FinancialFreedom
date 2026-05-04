using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FinancialFreedom.Shared;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace FinancialFreedom.Client.Pages;

public partial class WhatIf
{
    [Inject] private HttpClient Http { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    private ProjectionAssumptionsDto? _baseline;
    private IncomeReplacementResultDto? _scenario;
    private bool _busy;

    protected override async Task OnInitializedAsync()
    {
        _baseline = await Http.GetFromJsonAsync<ProjectionAssumptionsDto>("api/projection-assumptions");
    }

    private async Task RunWhatIf()
    {
        if (_baseline is null)
            return;
        _busy = true;
        _scenario = null;
        try
        {
            var res = await Http.PostAsJsonAsync("api/what-if/income-replacement", _baseline);
            res.EnsureSuccessStatusCode();
            _scenario = await res.Content.ReadFromJsonAsync<IncomeReplacementResultDto>();
        }
        catch (Exception ex)
        {
            Snackbar.Add(ex.Message, Severity.Error);
        }
        finally
        {
            _busy = false;
        }
    }
}
