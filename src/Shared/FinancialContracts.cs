namespace FinancialFreedom.Shared;

public record FamilyMemberDto(
    Guid Id,
    string Name,
    int Type,
    DateOnly DateOfBirth,
    int DisplayOrder);

public record FamilyMemberUpsertDto(
    string Name,
    int Type,
    DateOnly DateOfBirth,
    int DisplayOrder);

public record RetirementProfileDto(
    Guid FamilyMemberId,
    decimal Salary,
    decimal ContributionPercent,
    int RetirementAge,
    decimal EstimatedSalaryIncreasePercent,
    bool HasRetirementCatchup,
    decimal CompanyMatchPercent,
    decimal CompanyMatchEndsAtSalaryPercent,
    decimal SocialSecurityMonthlyBenefit,
    decimal ProjectedSocialSecurityMonthlyBenefit);

public record HsaProfileDto(
    Guid FamilyMemberId,
    bool HasHsa,
    decimal AnnualHsaContribution,
    bool HasHsaCatchup);

public sealed class ProjectionAssumptionsDto
{
    public decimal RetirementRateOfReturnPercent { get; set; }
    public decimal RetirementWithdrawalRatePercent { get; set; }
    public decimal HsaRateOfReturnPercent { get; set; }
    public decimal TaxableRateOfReturnPercent { get; set; }
    public decimal EducationRateOfReturnPercent { get; set; }
    public decimal EmergencyRateOfReturnPercent { get; set; }
    public decimal InflationRatePercent { get; set; }
    public int LifeExpectancyAge { get; set; }
    public decimal ReplacementRateGreenThresholdPercent { get; set; }
    public decimal ReplacementRateYellowThresholdPercent { get; set; }
}

public record AssetCategoryDto(Guid Id, string Name, bool IsRetirement, bool IsHsa, bool IsEducation, bool IsEmergency, bool IsCash, int DisplayOrder);

public record AccountOwnerDto(Guid FamilyMemberId, decimal OwnershipPercent);

public record AccountDto(
    Guid Id,
    Guid AssetCategoryId,
    string FinancialCompany,
    string AccountName,
    int AssetClass,
    int TaxTreatment,
    decimal CurrentBalance,
    bool IncludeInRetirementProjections,
    string? Notes,
    Guid? LinkedLiabilityId,
    IReadOnlyList<AccountOwnerDto> Owners);

public record AccountUpsertDto(
    Guid AssetCategoryId,
    string FinancialCompany,
    string AccountName,
    int AssetClass,
    int TaxTreatment,
    decimal CurrentBalance,
    bool IncludeInRetirementProjections,
    string? Notes,
    Guid? LinkedLiabilityId,
    IReadOnlyList<AccountOwnerDto> Owners);

public record LiabilityDto(Guid Id, string Name, int LiabilityType, decimal CurrentBalance);

public record LiabilityUpsertDto(string Name, int LiabilityType, decimal CurrentBalance);

public record GoalDto(Guid Id, string Name, decimal TargetAmount, DateOnly? TargetDate, Guid? FundingAccountId);

public record GoalUpsertDto(string Name, decimal TargetAmount, DateOnly? TargetDate, Guid? FundingAccountId);

public record DashboardCategoryTotalDto(string CategoryName, decimal Amount, string? ColorHint);

public record DashboardDto(
    decimal TotalNetWorth,
    IReadOnlyList<DashboardCategoryTotalDto> ByCategory);

public record SavingsByAgeResultDto(
    int OldestAdultAge,
    decimal SavingFactorMultiple,
    decimal CombinedSalary,
    decimal CurrentRetirementAssets,
    decimal TargetRetirementSavings,
    decimal SuccessRatePercent);

public record IncomeReplacementLinePointDto(
    int Year,
    decimal DadBalance,
    decimal MomBalance,
    decimal AllBalance,
    decimal DadSalary,
    decimal MomSalary,
    decimal CombinedSalary);

public record IncomeReplacementResultDto(
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
    int TrafficLight, // 0 red, 1 yellow, 2 green
    decimal ReplacementGreenThresholdPercent,
    decimal ReplacementYellowThresholdPercent,
    decimal RetirementReturnPercentAssumed,
    decimal HsaReturnPercentAssumed,
    decimal InflationRatePercentAssumed,
    IReadOnlyList<IncomeReplacementLinePointDto> ChartPoints);

public record NetWorthRowDto(
    string Name,
    string Category,
    decimal Amount,
    decimal PercentOfTotal,
    decimal? PriorYearAmount,
    decimal? YearOverYearPercent);

public record NetWorthSummaryDto(
    DateOnly AsOfDate,
    decimal TotalAssets,
    decimal TotalLiabilities,
    decimal NetWorthLiquid,
    decimal NetWorthIncludingRealEstate,
    IReadOnlyList<NetWorthRowDto> AssetRows,
    IReadOnlyList<NetWorthRowDto> SemiAssetRows,
    IReadOnlyList<NetWorthRowDto> LiabilityRows,
    IReadOnlyList<NetWorthRowDto> GoalRows,
    IReadOnlyList<DashboardCategoryTotalDto> RetirementTaxSlice);

public record TimelinePointDto(DateOnly Date, string CategoryName, decimal Amount);

public record TimelineSeriesDto(IReadOnlyList<TimelinePointDto> Points);

public record IrsLimitDto(int Year, int LimitType, decimal Amount);

public record SavingsByAgeMultipleDto(int Age, decimal Multiple);
