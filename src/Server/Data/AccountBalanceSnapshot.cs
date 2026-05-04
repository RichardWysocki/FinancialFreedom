namespace FinancialFreedom.Server.Data;

public class AccountBalanceSnapshot
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public Account Account { get; set; } = null!;

    public DateOnly AsOfDate { get; set; }
    public decimal Balance { get; set; }
    public SnapshotSource Source { get; set; } = SnapshotSource.Manual;
}
