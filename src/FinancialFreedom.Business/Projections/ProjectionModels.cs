namespace FinancialFreedom.Business.Projections;

public sealed record AdultProjectionInput(
    string Name,
    int CurrentAge,
    int RetirementAge,
    decimal CurrentSalary,
    decimal SalaryIncreasePercentPerYear,
    decimal RetirementBalance,
    decimal HsaBalance,
    decimal ContributionPercentOfSalary,
    bool UseCatchUp50,
    decimal CompanyMatchPercentOfEmployeeContribution,
    decimal CompanyMatchCapPercentOfSalary,
    decimal SocialSecurityMonthlyBenefit,
    decimal AnnualHsaContribution,
    bool UseHsaCatchUp55,
    int CurrentYear);

public sealed record HouseholdProjectionInput(
    IReadOnlyList<AdultProjectionInput> Adults,
    decimal RetirementReturnPercentPerYear,
    decimal HsaReturnPercentPerYear,
    decimal WithdrawalRatePercent,
    int LifeExpectancyAge,
    decimal ReplacementGreenPercent,
    decimal ReplacementYellowPercent,
    int ProjectionEndYear);

public sealed record YearBalancePoint(int Year, string SeriesName, decimal Balance);

/// <summary>Per-member annual salary while still working (0 after retirement age).</summary>
public sealed record YearSalaryPoint(int Year, string Name, decimal AnnualSalary);

public sealed record IncomeReplacementOutput(
    decimal EstimatedFutureFamilySalary,
    decimal AnnualRetirementContributions,
    decimal AnnualHealthSavingsContributions,
    decimal EstimatedReplacementAnnualSalary,
    decimal FutureRetirementSavingsApprox,
    decimal RetirementWithdrawAnnual,
    decimal FutureHealthSavingsApprox,
    decimal HealthWithdrawAnnual,
    decimal EstimatedSocialSecurityAnnual,
    decimal EstimatedTotalRetirementIncome,
    decimal ReplacementPercent,
    int TrafficLight,
    IReadOnlyList<YearBalancePoint> ChartPoints,
    IReadOnlyList<YearSalaryPoint> SalaryPoints);
