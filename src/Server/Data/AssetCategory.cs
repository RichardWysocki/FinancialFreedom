namespace FinancialFreedom.Server.Data;

public class AssetCategory
{
    public Guid Id { get; set; }
    public Guid HouseholdId { get; set; }
    public Household Household { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public bool IsRetirement { get; set; }
    public bool IsHsa { get; set; }
    public bool IsEducation { get; set; }
    public bool IsEmergency { get; set; }
    public bool IsCash { get; set; }
    public int DisplayOrder { get; set; }

    public ICollection<Account> Accounts { get; set; } = new List<Account>();
}
