using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FinancialFreedom.Shared;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace FinancialFreedom.Client.Pages.Settings;

public partial class ManageFamily
{
    [Inject] private HttpClient Http { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    private List<FamilyMemberDto> _members = [];
    private bool _dialogOpen;
    private FamilyMemberDto? _editing;
    private string _formName = "";
    private int _formType;

    /// <summary>HTML date input value (yyyy-MM-dd).</summary>
    private string _formDobIso = "";

    protected override async Task OnInitializedAsync() => await Reload();

    private async Task Reload()
    {
        _members = await Http.GetFromJsonAsync<List<FamilyMemberDto>>("api/family-members") ?? [];
    }

    private static string MemberTypeName(int t) => t == 0 ? "Adult" : "Kid";

    private void OpenEdit(FamilyMemberDto? m)
    {
        _editing = m;
        if (m is null)
        {
            _formName = "";
            _formType = 0;
            _formDobIso = DateTime.Today.AddYears(-35).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }
        else
        {
            _formName = m.Name;
            _formType = m.Type;
            _formDobIso = m.DateOfBirth.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        _dialogOpen = true;
    }

    private async Task SaveDialog()
    {
        if (string.IsNullOrWhiteSpace(_formName) ||
            string.IsNullOrWhiteSpace(_formDobIso) ||
            !DateOnly.TryParse(_formDobIso, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dob))
        {
            Snackbar.Add("Name and a valid date of birth are required.", Severity.Warning);
            return;
        }
        var body = new FamilyMemberUpsertDto(_formName.Trim(), _formType, dob, _editing?.DisplayOrder ?? _members.Count);

        try
        {
            if (_editing is null)
            {
                var res = await Http.PostAsJsonAsync("api/family-members", body);
                res.EnsureSuccessStatusCode();
            }
            else
            {
                var res = await Http.PutAsJsonAsync($"api/family-members/{_editing.Id}", body);
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

    private async Task Delete(FamilyMemberDto m)
    {
        try
        {
            var res = await Http.DeleteAsync($"api/family-members/{m.Id}");
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
