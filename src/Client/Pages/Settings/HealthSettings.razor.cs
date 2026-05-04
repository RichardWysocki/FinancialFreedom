using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FinancialFreedom.Shared;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace FinancialFreedom.Client.Pages.Settings;

public partial class HealthSettings
{
    [Inject] private HttpClient Http { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    private List<HsaProfileDto> _rows = [];
    private List<FamilyMemberDto> _members = [];
    private bool _loading = true;
    private bool _dialogOpen;
    private Guid _memberId;
    private bool _hasHsa;
    private decimal _annual;
    private bool _hsaCatchup;

    protected override async Task OnInitializedAsync() => await Reload();

    private async Task Reload()
    {
        _loading = true;
        _members = await Http.GetFromJsonAsync<List<FamilyMemberDto>>("api/family-members") ?? [];
        _rows = await Http.GetFromJsonAsync<List<HsaProfileDto>>("api/hsa-profiles") ?? [];
        _loading = false;
    }

    private string Name(Guid id) => _members.FirstOrDefault(m => m.Id == id)?.Name ?? id.ToString()[..8];

    private void Edit(HsaProfileDto row)
    {
        _memberId = row.FamilyMemberId;
        _hasHsa = row.HasHsa;
        _annual = row.AnnualHsaContribution;
        _hsaCatchup = row.HasHsaCatchup;
        _dialogOpen = true;
    }

    private async Task Save()
    {
        try
        {
            var dto = new HsaProfileDto(_memberId, _hasHsa, _annual, _hsaCatchup);
            var res = await Http.PutAsJsonAsync($"api/hsa-profiles/{_memberId}", dto);
            res.EnsureSuccessStatusCode();
            _dialogOpen = false;
            await Reload();
            Snackbar.Add("Saved.", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add(ex.Message, Severity.Error);
        }
    }
}
