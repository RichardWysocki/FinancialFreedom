using Microsoft.AspNetCore.Identity;

namespace FinancialFreedom.Server.Data;

public class ApplicationUser : IdentityUser
{
    /// <summary>UTC time of the last successful sign-in (cookie issued).</summary>
    public DateTimeOffset? LastLoginAt { get; set; }
}
