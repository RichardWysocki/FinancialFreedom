using System;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FinancialFreedom.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace FinancialFreedom.Client.Shared;

public partial class UserAccountStrip : IDisposable
{
    [Inject] private HttpClient Http { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    private CurrentUserProfileDto? _profile;
    private string? _loadError;

    private string DisplayName =>
        !string.IsNullOrEmpty(_profile?.Email)
            ? _profile.Email
            : _profile?.UserName ?? "User";

    protected override async Task OnInitializedAsync()
    {
        AuthenticationStateProvider.AuthenticationStateChanged += OnAuthChanged;
        await LoadIfSignedInAsync();
    }

    private void OnAuthChanged(Task<AuthenticationState> authStateTask)
    {
        _ = HandleAuthChangedAsync(authStateTask);
    }

    private async Task HandleAuthChangedAsync(Task<AuthenticationState> authStateTask)
    {
        var state = await authStateTask;
        if (state.User.Identity?.IsAuthenticated is true)
        {
            await InvokeAsync(ReloadAsync);
        }
        else
        {
            await InvokeAsync(() =>
            {
                _profile = null;
                _loadError = null;
                StateHasChanged();
            });
        }
    }

    private async Task ReloadAsync()
    {
        _profile = null;
        _loadError = null;
        await LoadIfSignedInAsync();
        StateHasChanged();
    }

    private async Task LoadIfSignedInAsync()
    {
        var state = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        if (state.User.Identity?.IsAuthenticated is not true)
            return;

        try
        {
            _profile = await Http.GetFromJsonAsync<CurrentUserProfileDto>("api/me");
            _loadError = null;
        }
        catch
        {
            _loadError = "Could not load profile.";
        }
    }

    private static string FormatLastLogin(DateTimeOffset? utc)
    {
        if (utc is null)
            return "— (first visit after this update)";

        var local = utc.Value.ToLocalTime();
        return local.ToString("g", CultureInfo.CurrentCulture);
    }

    public void Dispose()
    {
        AuthenticationStateProvider.AuthenticationStateChanged -= OnAuthChanged;
    }
}
