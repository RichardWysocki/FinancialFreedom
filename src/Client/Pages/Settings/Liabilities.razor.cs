using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FinancialFreedom.Shared;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace FinancialFreedom.Client.Pages.Settings;

public partial class Liabilities
{
    [Inject] private HttpClient Http { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    private List<LiabilityDto> _rows = [];
    private bool _dialog;
    private Guid? _editId;
    private string _name = "";
    private int _type;
    private decimal _balance;

    protected override async Task OnInitializedAsync() => await Reload();

    private async Task Reload() => _rows = await Http.GetFromJsonAsync<List<LiabilityDto>>("api/liabilities") ?? [];

    private void Open(LiabilityDto? x)
    {
        _editId = x?.Id;
        _name = x?.Name ?? "";
        _type = x?.LiabilityType ?? 0;
        _balance = x?.CurrentBalance ?? 0;
        _dialog = true;
    }

    private async Task Save()
    {
        var body = new LiabilityUpsertDto(_name, _type, _balance);
        try
        {
            if (_editId is null)
                (await Http.PostAsJsonAsync("api/liabilities", body)).EnsureSuccessStatusCode();
            else
                (await Http.PutAsJsonAsync($"api/liabilities/{_editId}", body)).EnsureSuccessStatusCode();
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
            (await Http.DeleteAsync($"api/liabilities/{id}")).EnsureSuccessStatusCode();
            await Reload();
        }
        catch (Exception ex)
        {
            Snackbar.Add(ex.Message, Severity.Error);
        }
    }
}
