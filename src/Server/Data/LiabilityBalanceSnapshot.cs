namespace FinancialFreedom.Server.Data;

public class LiabilityBalanceSnapshot
{
    public Guid Id { get; set; }
    public Guid LiabilityId { get; set; }
    public Liability Liability { get; set; } = null!;

    public DateOnly AsOfDate { get; set; }
    public decimal Balance { get; set; }
    public SnapshotSource Source { get; set; } = SnapshotSource.Manual;
}
