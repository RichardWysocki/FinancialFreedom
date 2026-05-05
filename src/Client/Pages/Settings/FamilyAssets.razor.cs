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

public partial class FamilyAssets
{
    /// <summary>USD so Balance shows $ — invariant &quot;C&quot; yields ¤.</summary>
    private static readonly CultureInfo Usd = CultureInfo.GetCultureInfo("en-US");

    private static readonly DialogOptions _dialogOptions = new()
    {
        MaxWidth = MaxWidth.ExtraLarge,
        FullWidth = true,
        // Do not close on outside click / Escape — user loses unsaved form data.
        BackdropClick = false,
        CloseOnEscapeKey = false,
    };

    [Inject] private HttpClient Http { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    private List<AccountDto> _accounts = [];
    private List<AssetCategoryDto> _categories = [];
    private List<FamilyMemberDto> _members = [];
    private bool _loading = true;
    private bool _dialogOpen;
    private Guid? _editId;

    private string _name = "";
    private string _institution = "";
    private Guid _categoryId;
    private int _assetClass;
    private int _tax;
    private decimal _balance;
    private Guid _ownerMemberId;

    private sealed record OwnerSlice(string Name, decimal Amount, string Color);

    private static readonly string[] OwnerChartColors =
    [
        "#594ae2", "#ff9800", "#4caf50", "#00bcd4", "#e91e63", "#9c27b0",
        "#795548", "#607d8b",
    ];

    private List<OwnerSlice> _ownerSlices = [];
    private string? _ownerPieStyle;

    protected override async Task OnInitializedAsync() => await Reload();

    private async Task Reload()
    {
        _loading = true;
        _categories = await Http.GetFromJsonAsync<List<AssetCategoryDto>>("api/asset-categories") ?? [];
        _members = await Http.GetFromJsonAsync<List<FamilyMemberDto>>("api/family-members") ?? [];
        _accounts = await Http.GetFromJsonAsync<List<AccountDto>>("api/accounts") ?? [];
        if (_categories.Count > 0 && _categoryId == Guid.Empty)
            _categoryId = _categories[0].Id;
        RefreshOwnerDistribution();
        _loading = false;
    }

    private string CategoryName(Guid id) => _categories.FirstOrDefault(c => c.Id == id)?.Name ?? "?";

    private string MemberName(Guid id) => _members.FirstOrDefault(m => m.Id == id)?.Name ?? "—";

    private static string AssetClassName(int c) => c == 0 ? "Liquid" : "Semi-liquid";

    private static string TaxTreatmentName(int t) => t switch
    {
        0 => "Pre-tax",
        1 => "Roth",
        2 => "Taxable",
        3 => "HSA",
        4 => "Education",
        _ => "?",
    };

    private string OwnersCell(AccountDto a) =>
        string.Join(" · ", a.Owners
            .OrderByDescending(o => o.OwnershipPercent)
            .Select(o => $"{MemberName(o.FamilyMemberId)} ({o.OwnershipPercent.ToString("0.##", CultureInfo.InvariantCulture)}%)"));

    private void RefreshOwnerDistribution()
    {
        var totals = new Dictionary<Guid, decimal>();
        foreach (var account in _accounts)
        {
            foreach (var o in account.Owners)
            {
                var share = account.CurrentBalance * (o.OwnershipPercent / 100m);
                totals[o.FamilyMemberId] = totals.GetValueOrDefault(o.FamilyMemberId) + share;
            }
        }

        _ownerSlices = totals
            .Where(kv => kv.Value > 0)
            .OrderByDescending(kv => kv.Value)
            .Select((kv, i) => new OwnerSlice(MemberName(kv.Key), kv.Value, OwnerChartColors[i % OwnerChartColors.Length]))
            .ToList();

        var sum = _ownerSlices.Sum(s => s.Amount);
        if (sum <= 0)
        {
            _ownerPieStyle = null;
            return;
        }

        var parts = new List<string>();
        double start = 0;
        foreach (var s in _ownerSlices)
        {
            var sweep = (double)(s.Amount / sum * 360m);
            var end = start + sweep;
            parts.Add($"{s.Color} {start.ToString("F1", CultureInfo.InvariantCulture)}deg {end.ToString("F1", CultureInfo.InvariantCulture)}deg");
            start = end;
        }

        _ownerPieStyle = $"conic-gradient({string.Join(", ", parts)})";
    }

    private double OwnerPctOfAttributed(OwnerSlice s)
    {
        var t = _ownerSlices.Sum(x => x.Amount);
        return t > 0 ? (double)(s.Amount / t * 100m) : 0;
    }

    private static string UsdMoney(decimal value) => value.ToString("C2", Usd);

    private void CloseDialog() => _dialogOpen = false;

    private void Open(AccountDto? acc)
    {
        _editId = acc?.Id;
        if (acc is null)
        {
            _name = "";
            _institution = "";
            _categoryId = _categories.FirstOrDefault()?.Id ?? Guid.Empty;
            _assetClass = 0;
            _tax = 2;
            _balance = 0;
            _ownerMemberId = _members.FirstOrDefault()?.Id ?? Guid.Empty;
        }
        else
        {
            _name = acc.AccountName;
            _institution = acc.FinancialCompany;
            _categoryId = acc.AssetCategoryId;
            _assetClass = acc.AssetClass;
            _tax = acc.TaxTreatment;
            _balance = acc.CurrentBalance;
            _ownerMemberId = acc.Owners.Count > 0
                ? acc.Owners.OrderByDescending(o => o.OwnershipPercent).ThenBy(o => o.FamilyMemberId).First().FamilyMemberId
                : _members.FirstOrDefault()?.Id ?? Guid.Empty;
        }

        _dialogOpen = true;
    }

    private async Task Save()
    {
        if (_members.Count == 0)
        {
            Snackbar.Add("Add family members before assigning account ownership.", Severity.Warning);
            return;
        }

        if (_ownerMemberId == Guid.Empty || _members.All(m => m.Id != _ownerMemberId))
        {
            Snackbar.Add("Select an owner for this account.", Severity.Warning);
            return;
        }

        var owners = new List<AccountOwnerDto> { new(_ownerMemberId, 100m) };

        var body = new AccountUpsertDto(
            _categoryId,
            _institution.Trim(),
            _name.Trim(),
            _assetClass,
            _tax,
            _balance,
            true,
            null,
            null,
            owners);

        try
        {
            if (_editId is null)
            {
                var res = await Http.PostAsJsonAsync("api/accounts", body);
                res.EnsureSuccessStatusCode();
            }
            else
            {
                var res = await Http.PutAsJsonAsync($"api/accounts/{_editId}", body);
                res.EnsureSuccessStatusCode();
            }

            _dialogOpen = false;
            await Reload();
            Snackbar.Add("Saved.", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add(ex.Message, Severity.Error);
        }
    }

    private async Task Delete(AccountDto acc)
    {
        try
        {
            var res = await Http.DeleteAsync($"api/accounts/{acc.Id}");
            res.EnsureSuccessStatusCode();
            await Reload();
            Snackbar.Add("Deleted.", Severity.Info);
        }
        catch (Exception ex)
        {
            Snackbar.Add(ex.Message, Severity.Error);
        }
    }
}
