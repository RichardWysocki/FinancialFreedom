namespace FinancialFreedom.Server.Data;

public class ProjectionAssumptions
{
    public Guid HouseholdId { get; set; }
    public Household Household { get; set; } = null!;

    public decimal RetirementRateOfReturnPercent { get; set; } = 8m;
    public decimal RetirementWithdrawalRatePercent { get; set; } = 4m;
    public decimal HsaRateOfReturnPercent { get; set; } = 5m;
    public decimal TaxableRateOfReturnPercent { get; set; } = 6m;
    public decimal EducationRateOfReturnPercent { get; set; } = 5m;
    public decimal EmergencyRateOfReturnPercent { get; set; } = 4m;
    public decimal InflationRatePercent { get; set; } = 2.5m;
    public int LifeExpectancyAge { get; set; } = 95;
    public decimal ReplacementRateGreenThresholdPercent { get; set; } = 85m;
    public decimal ReplacementRateYellowThresholdPercent { get; set; } = 70m;
}
