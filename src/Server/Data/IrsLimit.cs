namespace FinancialFreedom.Server.Data;

/// <summary>Global IRS / regulatory limits by calendar year (not household-scoped).</summary>
public class IrsLimit
{
    public int Id { get; set; }
    public int Year { get; set; }
    public IrsLimitType LimitType { get; set; }
    public decimal Amount { get; set; }
}
