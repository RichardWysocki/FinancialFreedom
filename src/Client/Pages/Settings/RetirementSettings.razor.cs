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
    private ProjectionAssumptionsDto? _assumptions;
    private bool _loading = true;
    private bool _dialogOpen;
    private Guid _editMemberId;
    private string _editMemberName = "";
    private DateOnly _editMemberDob;

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
        try
        {
            var membersTask = Http.GetFromJsonAsync<List<FamilyMemberDto>>("api/family-members");
            var rowsTask = Http.GetFromJsonAsync<List<RetirementProfileDto>>("api/retirement-profiles");
            var assumptionsTask = Http.GetFromJsonAsync<ProjectionAssumptionsDto>("api/projection-assumptions");
            await Task.WhenAll(membersTask, rowsTask, assumptionsTask);
            _members = await membersTask ?? [];
            _rows = await rowsTask ?? [];
            _assumptions = await assumptionsTask;
        }
        finally
        {
            _loading = false;
        }
    }

    private string Name(Guid id) => _members.FirstOrDefault(m => m.Id == id)?.Name ?? id.ToString()[..8];

    private decimal InflationRatePercent => _assumptions?.InflationRatePercent ?? 2.5m;

    private DateOnly AsOfTodayLocal => DateOnly.FromDateTime(DateTime.Today);

    private int YearsToRetirementForDisplay
    {
        get
        {
            var age = RetirementSocialSecurityProjection.AgeCompletedYears(_editMemberDob, AsOfTodayLocal);
            return Math.Max(0, _retirementAge - age);
        }
    }

    private decimal FutureSocialSecurityMonthly =>
        RetirementSocialSecurityProjection.ProjectedMonthly(
            _ssMonthly, InflationRatePercent, _editMemberDob, _retirementAge, AsOfTodayLocal);

    private string FutureSsHelperText =>
        $"At retirement (~{YearsToRetirementForDisplay} year(s) from now), using {InflationRatePercent.ToString("F2", CultureInfo.InvariantCulture)}% inflation from Generic Settings.";

    private static string UsdMoney(decimal value) => value.ToString("C2", Usd);

    private static string PctOneDecimal(decimal value) => value.ToString("F1", CultureInfo.InvariantCulture);

    private void Edit(RetirementProfileDto row)
    {
        _editMemberId = row.FamilyMemberId;
        var member = _members.FirstOrDefault(m => m.Id == row.FamilyMemberId);
        _editMemberName = member?.Name ?? Name(row.FamilyMemberId);
        _editMemberDob = member?.DateOfBirth ?? DateOnly.FromDateTime(DateTime.Today.AddYears(-35));
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
            var projectedSs = RetirementSocialSecurityProjection.ProjectedMonthly(
                _ssMonthly, InflationRatePercent, _editMemberDob, _retirementAge, AsOfTodayLocal);
            var dto = new RetirementProfileDto(
                _editMemberId,
                _salary,
                _contributionPct,
                _retirementAge,
                _raisePct,
                _catchup,
                _matchPct,
                _matchCapPct,
                _ssMonthly,
                projectedSs);
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
