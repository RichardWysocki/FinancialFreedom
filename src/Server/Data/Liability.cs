namespace FinancialFreedom.Server.Data;

public class Liability
{
    public Guid Id { get; set; }
    public Guid HouseholdId { get; set; }
    public Household Household { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public LiabilityType LiabilityType { get; set; }
    public decimal CurrentBalance { get; set; }

    public ICollection<LiabilityBalanceSnapshot> BalanceSnapshots { get; set; } = new List<LiabilityBalanceSnapshot>();
    public ICollection<Account> LinkedAccounts { get; set; } = new List<Account>();
}
