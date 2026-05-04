namespace FinancialFreedom.Server.Data;

public class AccountOwner
{
    public Guid AccountId { get; set; }
    public Account Account { get; set; } = null!;

    public Guid FamilyMemberId { get; set; }
    public FamilyMember FamilyMember { get; set; } = null!;

    /// <summary>Ownership share 0–100; joint accounts typically 50/50.</summary>
    public decimal OwnershipPercent { get; set; } = 100m;
}
