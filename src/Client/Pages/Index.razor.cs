using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FinancialFreedom.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using MudBlazor;

namespace FinancialFreedom.Client.Pages;

public partial class Index
{
    [Inject] private HttpClient Http { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthState { get; set; } = default!;

    private readonly List<FamilyMemberDto> _members = [];
    private DashboardDto? _dashboard;
    private string? _loadError;
    private string? _ownerFilter;
    private bool _busy;
    private bool _isAuthenticated;

    protected override async Task OnInitializedAsync()
    {
        var state = await AuthState.GetAuthenticationStateAsync();
        _isAuthenticated = state.User.Identity?.IsAuthenticated == true;
        if (!_isAuthenticated)
            return;

        await ReloadMembersAsync();
        await ReloadDashboardAsync();
    }

    private bool HasFinancialData()
    {
        if (_dashboard is null)
            return false;
        if (_dashboard.TotalNetWorth != 0)
            return true;
        return _dashboard.ByCategory.Count > 0;
    }

    private async Task OnOwnerChanged(string value)
    {
        _ownerFilter = string.IsNullOrWhiteSpace(value) ? null : value;
        await ReloadDashboardAsync();
    }

    private async Task ReloadMembersAsync()
    {
        try
        {
            var list = await Http.GetFromJsonAsync<List<FamilyMemberDto>>("api/family-members");
            _members.Clear();
            if (list is not null)
                _members.AddRange(list);
        }
        catch
        {
            /* ignore */
        }
    }

    private async Task ReloadDashboardAsync()
    {
        _loadError = null;
        _dashboard = null;
        try
        {
            var url = string.IsNullOrEmpty(_ownerFilter)
                ? "api/dashboard"
                : $"api/dashboard?owner={Uri.EscapeDataString(_ownerFilter)}";
            _dashboard = await Http.GetFromJsonAsync<DashboardDto>(url);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            _loadError = null;
        }
        catch (Exception ex)
        {
            _loadError = ex.Message;
        }
    }

    private async Task SaveSnapshots()
    {
        _busy = true;
        try
        {
            var res = await Http.PostAsync("api/snapshots/save-all", null);
            res.EnsureSuccessStatusCode();
            Snackbar.Add("Balances saved for today.", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Save failed: {ex.Message}", Severity.Error);
        }
        finally
        {
            _busy = false;
        }
    }

    private async Task ExportExcel()
    {
        _busy = true;
        try
        {
            var bytes = await Http.GetByteArrayAsync("api/excel/export");
            var b64 = Convert.ToBase64String(bytes);
            await JS.InvokeVoidAsync("ff.download", "financial-freedom-accounts.xlsx", b64);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Export failed: {ex.Message}", Severity.Error);
        }
        finally
        {
            _busy = false;
        }
    }

    private async Task ImportExcel(InputFileChangeEventArgs e)
    {
        var file = e.File;
        if (file is null)
            return;

        _busy = true;
        try
        {
            await using var stream = file.OpenReadStream(maxAllowedSize: 10_000_000);
            using var content = new MultipartFormDataContent();
            content.Add(new StreamContent(stream), "file", file.Name);
            var res = await Http.PostAsync("api/excel/import", content);
            res.EnsureSuccessStatusCode();
            Snackbar.Add("Import completed.", Severity.Success);
            await ReloadDashboardAsync();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Import failed: {ex.Message}", Severity.Error);
        }
        finally
        {
            _busy = false;
        }
    }
}
