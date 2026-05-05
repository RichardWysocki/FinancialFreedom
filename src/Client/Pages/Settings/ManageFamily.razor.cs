using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
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

    /// <summary>Reads a user-facing message from a non-success API response (JSON body or plain text).</summary>
    private static async Task<string> ReadHttpErrorAsync(HttpResponseMessage response)
    {
        var text = (await response.Content.ReadAsStringAsync()).Trim();
        if (string.IsNullOrEmpty(text))
            return $"Request failed ({(int)response.StatusCode}).";

        try
        {
            using var doc = JsonDocument.Parse(text);
            var root = doc.RootElement;
            if (root.ValueKind == JsonValueKind.Object)
            {
                if (TryGetJsonString(root, "message", out var m)) return m;
                if (TryGetJsonString(root, "Message", out var m2)) return m2;
                if (TryGetJsonString(root, "detail", out var d)) return d;
                if (TryGetJsonString(root, "title", out var t)) return t;
            }
            else if (root.ValueKind == JsonValueKind.String)
            {
                var s = root.GetString();
                if (!string.IsNullOrWhiteSpace(s)) return s;
            }
        }
        catch (JsonException)
        {
            // Plain-text error body
        }

        return text;
    }

    private static bool TryGetJsonString(JsonElement root, string name, out string value)
    {
        value = "";
        if (!root.TryGetProperty(name, out var el) || el.ValueKind != JsonValueKind.String)
            return false;
        value = el.GetString() ?? "";
        return value.Length > 0;
    }

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
                if (!res.IsSuccessStatusCode)
                {
                    Snackbar.Add(await ReadHttpErrorAsync(res), Severity.Error);
                    return;
                }
            }
            else
            {
                var res = await Http.PutAsJsonAsync($"api/family-members/{_editing.Id}", body);
                if (!res.IsSuccessStatusCode)
                {
                    Snackbar.Add(await ReadHttpErrorAsync(res), Severity.Error);
                    return;
                }
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
            if (!res.IsSuccessStatusCode)
            {
                Snackbar.Add(await ReadHttpErrorAsync(res), Severity.Error);
                return;
            }

            await Reload();
            Snackbar.Add("Deleted.", Severity.Info);
        }
        catch (Exception ex)
        {
            Snackbar.Add(ex.Message, Severity.Error);
        }
    }
}
