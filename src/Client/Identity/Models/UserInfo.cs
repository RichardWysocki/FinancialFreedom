using System.Text.Json.Serialization;

namespace FinancialFreedom.Client.Identity.Models;

public sealed class UserInfo
{
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("isEmailConfirmed")]
    public bool IsEmailConfirmed { get; set; }
}
