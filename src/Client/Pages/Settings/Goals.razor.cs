using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FinancialFreedom.Shared;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace FinancialFreedom.Client.Pages.Settings;

public partial class Goals
{
    [Inject] private HttpClient Http { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    private List<GoalDto> _rows = [];
    private bool _dialog;
    private Guid? _editId;
    private string _name = "";
    private decimal _amount;
    private DateTime? _targetDate;

    protected override async Task OnInitializedAsync() => await Reload();

    private async Task Reload() => _rows = await Http.GetFromJsonAsync<List<GoalDto>>("api/goals") ?? [];

    private void Open(GoalDto? g)
    {
        _editId = g?.Id;
        _name = g?.Name ?? "";
        _amount = g?.TargetAmount ?? 0;
        _targetDate = g?.TargetDate?.ToDateTime(TimeOnly.MinValue);
        _dialog = true;
    }

    private async Task Save()
    {
        var td = _targetDate is null ? (DateOnly?)null : DateOnly.FromDateTime(_targetDate.Value);
        var body = new GoalUpsertDto(_name, _amount, td, null);
        try
        {
            if (_editId is null)
                (await Http.PostAsJsonAsync("api/goals", body)).EnsureSuccessStatusCode();
            else
                (await Http.PutAsJsonAsync($"api/goals/{_editId}", body)).EnsureSuccessStatusCode();
            _dialog = false;
            await Reload();
            Snackbar.Add("Saved.", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add(ex.Message, Severity.Error);
        }
    }

    private async Task Delete(Guid id)
    {
        try
        {
            (await Http.DeleteAsync($"api/goals/{id}")).EnsureSuccessStatusCode();
            await Reload();
        }
        catch (Exception ex)
        {
            Snackbar.Add(ex.Message, Severity.Error);
        }
    }
}
