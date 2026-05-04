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

public partial class FamilyAssets
{
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
    private readonly Dictionary<Guid, decimal> _ownerPct = new();

    protected override async Task OnInitializedAsync() => await Reload();

    private async Task Reload()
    {
        _loading = true;
        _categories = await Http.GetFromJsonAsync<List<AssetCategoryDto>>("api/asset-categories") ?? [];
        _members = await Http.GetFromJsonAsync<List<FamilyMemberDto>>("api/family-members") ?? [];
        _accounts = await Http.GetFromJsonAsync<List<AccountDto>>("api/accounts") ?? [];
        if (_categories.Count > 0 && _categoryId == Guid.Empty)
            _categoryId = _categories[0].Id;
        _loading = false;
    }

    private string CategoryName(Guid id) => _categories.FirstOrDefault(c => c.Id == id)?.Name ?? "?";

    private void Open(AccountDto? acc)
    {
        _editId = acc?.Id;
        _ownerPct.Clear();
        if (acc is null)
        {
            _name = "";
            _institution = "";
            _categoryId = _categories.FirstOrDefault()?.Id ?? Guid.Empty;
            _assetClass = 0;
            _tax = 2;
            _balance = 0;
            foreach (var m in _members)
                _ownerPct[m.Id] = _members.Count == 1 ? 100 : 0;
        }
        else
        {
            _name = acc.AccountName;
            _institution = acc.FinancialCompany;
            _categoryId = acc.AssetCategoryId;
            _assetClass = acc.AssetClass;
            _tax = acc.TaxTreatment;
            _balance = acc.CurrentBalance;
            foreach (var o in acc.Owners)
                _ownerPct[o.FamilyMemberId] = o.OwnershipPercent;
        }

        _dialogOpen = true;
    }

    private decimal OwnerPct(Guid memberId) => _ownerPct.GetValueOrDefault(memberId);

    private void SetOwnerPct(Guid memberId, decimal v) => _ownerPct[memberId] = v;

    private async Task Save()
    {
        var owners = _ownerPct
            .Where(kv => kv.Value > 0)
            .Select(kv => new AccountOwnerDto(kv.Key, kv.Value))
            .ToList();
        if (owners.Count == 0)
        {
            Snackbar.Add("Add at least one owner with a ownership percent.", Severity.Warning);
            return;
        }

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
