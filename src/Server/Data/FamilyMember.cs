namespace FinancialFreedom.Server.Data;

public class FamilyMember
{
    public Guid Id { get; set; }
    public Guid HouseholdId { get; set; }
    public Household Household { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public FamilyMemberType Type { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public int DisplayOrder { get; set; }

    public RetirementProfile? RetirementProfile { get; set; }
    public HsaProfile? HsaProfile { get; set; }
    public ICollection<AccountOwner> AccountOwners { get; set; } = new List<AccountOwner>();
}
