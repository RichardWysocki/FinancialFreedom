namespace FinancialFreedom.Server.Data;

public class Goal
{
    public Guid Id { get; set; }
    public Guid HouseholdId { get; set; }
    public Household Household { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    public DateOnly? TargetDate { get; set; }
    public Guid? FundingAccountId { get; set; }
    public Account? FundingAccount { get; set; }
}
