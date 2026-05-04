namespace FinancialFreedom.Server.Data;

/// <summary>One household per authenticated user (owner).</summary>
public class Household
{
    public Guid Id { get; set; }
    public string OwnerUserId { get; set; } = string.Empty;
    public ApplicationUser? Owner { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<FamilyMember> FamilyMembers { get; set; } = new List<FamilyMember>();
    public ICollection<AssetCategory> AssetCategories { get; set; } = new List<AssetCategory>();
    public ICollection<Account> Accounts { get; set; } = new List<Account>();
    public ICollection<Liability> Liabilities { get; set; } = new List<Liability>();
    public ICollection<Goal> Goals { get; set; } = new List<Goal>();
    public ProjectionAssumptions? ProjectionAssumptions { get; set; }
}
