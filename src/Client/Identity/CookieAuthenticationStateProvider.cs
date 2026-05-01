using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using FinancialFreedom.Client.Identity.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace FinancialFreedom.Client.Identity;

public sealed class CookieAuthenticationStateProvider(
    IHttpClientFactory httpClientFactory,
    ILogger<CookieAuthenticationStateProvider> logger)
    : AuthenticationStateProvider, IAccountManagement
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly HttpClient _http = httpClientFactory.CreateClient("WebApi");
    private bool _authenticated;
    private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    public async Task<FormResult> RegisterAsync(string email, string password)
    {
        const string unknown = "An unknown error prevented registration from succeeding.";
        try
        {
            var result = await _http.PostAsJsonAsync("register", new { email, password });
            if (result.IsSuccessStatusCode)
                return new FormResult { Succeeded = true };

            var details = await result.Content.ReadAsStringAsync();
            var errors = new List<string>();
            try
            {
                using var problemDetails = JsonDocument.Parse(details);
                if (problemDetails.RootElement.TryGetProperty("errors", out var errorList))
                {
                    foreach (var errorEntry in errorList.EnumerateObject())
                    {
                        if (errorEntry.Value.ValueKind == JsonValueKind.String)
                        {
                            var s = errorEntry.Value.GetString();
                            if (!string.IsNullOrEmpty(s)) errors.Add(s);
                        }
                        else if (errorEntry.Value.ValueKind == JsonValueKind.Array)
                        {
                            errors.AddRange(
                                errorEntry.Value.EnumerateArray()
                                    .Select(e => e.GetString())
                                    .Where(s => !string.IsNullOrEmpty(s))
                                    .Select(s => s!));
                        }
                    }
                }
            }
            catch (JsonException)
            {
                errors.Add(unknown);
            }

            return new FormResult { Succeeded = false, ErrorList = errors.Count > 0 ? errors : [unknown] };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Registration request failed.");
            return new FormResult { Succeeded = false, ErrorList = [unknown] };
        }
    }

    public async Task<FormResult> LoginAsync(string email, string password)
    {
        try
        {
            var result = await _http.PostAsJsonAsync("login?useCookies=true", new { email, password });
            if (result.IsSuccessStatusCode)
            {
                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
                return new FormResult { Succeeded = true };
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Login request failed.");
        }

        return new FormResult { Succeeded = false, ErrorList = ["Invalid email and/or password."] };
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        _authenticated = false;
        var user = _anonymous;

        try
        {
            using var userResponse = await _http.GetAsync("manage/info");
            userResponse.EnsureSuccessStatusCode();

            var userJson = await userResponse.Content.ReadAsStringAsync();
            var userInfo = JsonSerializer.Deserialize<UserInfo>(userJson, JsonOptions);
            if (userInfo is null)
                return new AuthenticationState(user);

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, userInfo.Email),
                new(ClaimTypes.Email, userInfo.Email),
            };

            try
            {
                using var rolesResponse = await _http.GetAsync("roles");
                rolesResponse.EnsureSuccessStatusCode();
                var rolesJson = await rolesResponse.Content.ReadAsStringAsync();
                var roles = JsonSerializer.Deserialize<RoleClaimDto[]>(rolesJson, JsonOptions);
                if (roles is { Length: > 0 })
                {
                    foreach (var role in roles)
                    {
                        if (!string.IsNullOrEmpty(role.Type) && !string.IsNullOrEmpty(role.Value))
                            claims.Add(new Claim(role.Type, role.Value, role.ValueType, role.Issuer, role.OriginalIssuer));
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Could not load role claims; continuing with base identity.");
            }

            var id = new ClaimsIdentity(claims, nameof(CookieAuthenticationStateProvider));
            user = new ClaimsPrincipal(id);
            _authenticated = true;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            // expected when not logged in
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Reading authentication state failed.");
        }

        return new AuthenticationState(user);
    }

    public async Task LogoutAsync()
    {
        const string empty = "{}";
        await _http.PostAsync("logout", new StringContent(empty, Encoding.UTF8, "application/json"));
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task<bool> CheckAuthenticatedAsync()
    {
        await GetAuthenticationStateAsync();
        return _authenticated;
    }

    private sealed class RoleClaimDto
    {
        public string? Issuer { get; set; }
        public string? OriginalIssuer { get; set; }
        public string? Type { get; set; }
        public string? Value { get; set; }
        public string? ValueType { get; set; }
    }
}
