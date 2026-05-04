namespace FinancialFreedom.Server.Data;

public class RetirementProfile
{
    public Guid FamilyMemberId { get; set; }
    public FamilyMember FamilyMember { get; set; } = null!;

    public decimal Salary { get; set; }
    public decimal ContributionPercent { get; set; }
    public int RetirementAge { get; set; } = 67;
    public decimal EstimatedSalaryIncreasePercent { get; set; }
    public bool HasRetirementCatchup { get; set; }
    public decimal CompanyMatchPercent { get; set; }
    public decimal CompanyMatchEndsAtSalaryPercent { get; set; }
    public decimal SocialSecurityMonthlyBenefit { get; set; }
}
