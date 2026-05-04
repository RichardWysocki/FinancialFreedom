namespace FinancialFreedom.Server.Data;

public class HsaProfile
{
    public Guid FamilyMemberId { get; set; }
    public FamilyMember FamilyMember { get; set; } = null!;

    public bool HasHsa { get; set; }
    public decimal AnnualHsaContribution { get; set; }
    public bool HasHsaCatchup { get; set; }
}
