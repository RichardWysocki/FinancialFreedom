namespace FinancialFreedom.Shared;

public class CurrentUserProfileDto
{
    public Guid? HouseholdId { get; set; }
    public string? Email { get; set; }
    public string? UserName { get; set; }
    /// <summary>UTC timestamp of the last successful sign-in (updated when the auth cookie is issued).</summary>
    public DateTimeOffset? LastLoginAt { get; set; }
}
