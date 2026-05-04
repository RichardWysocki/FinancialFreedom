using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FinancialFreedom.Shared;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace FinancialFreedom.Client.Pages.Settings;

public partial class RetirementSettings
{
    /// <summary>USD so salary fields show $ — invariant "C" shows ¤ (generic currency).</summary>
    private static readonly CultureInfo Usd = CultureInfo.GetCultureInfo("en-US");

    private static readonly DialogOptions _editDialogOptions = new()
    {
        MaxWidth = MaxWidth.ExtraLarge,
        FullWidth = true,
    };

    [Inject] private HttpClient Http { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    private List<RetirementProfileDto> _rows = [];
    private List<FamilyMemberDto> _members = [];
    private bool _loading = true;
    private bool _dialogOpen;
    private Guid _editMemberId;

    private decimal _salary;
    private decimal _contributionPct;
    private int _retirementAge;
    private decimal _raisePct;
    private bool _catchup;
    private decimal _matchPct;
    private decimal _matchCapPct;
    private decimal _ssMonthly;

    protected override async Task OnInitializedAsync() => await Reload();

    private async Task Reload()
    {
        _loading = true;
        _members = await Http.GetFromJsonAsync<List<FamilyMemberDto>>("api/family-members") ?? [];
        _rows = await Http.GetFromJsonAsync<List<RetirementProfileDto>>("api/retirement-profiles") ?? [];
        _loading = false;
    }

    private string Name(Guid id) => _members.FirstOrDefault(m => m.Id == id)?.Name ?? id.ToString()[..8];

    private static string UsdMoney(decimal value) => value.ToString("C2", Usd);

    private static string PctOneDecimal(decimal value) => value.ToString("F1", CultureInfo.InvariantCulture);

    private void Edit(RetirementProfileDto row)
    {
        _editMemberId = row.FamilyMemberId;
        _salary = row.Salary;
        _contributionPct = row.ContributionPercent;
        _retirementAge = row.RetirementAge;
        _raisePct = row.EstimatedSalaryIncreasePercent;
        _catchup = row.HasRetirementCatchup;
        _matchPct = row.CompanyMatchPercent;
        _matchCapPct = row.CompanyMatchEndsAtSalaryPercent;
        _ssMonthly = row.SocialSecurityMonthlyBenefit;
        _dialogOpen = true;
    }

    private async Task Save()
    {
        try
        {
            var dto = new RetirementProfileDto(
                _editMemberId,
                _salary,
                _contributionPct,
                _retirementAge,
                _raisePct,
                _catchup,
                _matchPct,
                _matchCapPct,
                _ssMonthly);
            var res = await Http.PutAsJsonAsync($"api/retirement-profiles/{_editMemberId}", dto);
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
