namespace FinancialFreedom.Server.Data;

public class Account
{
    public Guid Id { get; set; }
    public Guid HouseholdId { get; set; }
    public Household Household { get; set; } = null!;

    public Guid AssetCategoryId { get; set; }
    public AssetCategory AssetCategory { get; set; } = null!;

    public string FinancialCompany { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public AssetClass AssetClass { get; set; } = AssetClass.Liquid;
    public TaxTreatment TaxTreatment { get; set; } = TaxTreatment.Taxable;
    public decimal CurrentBalance { get; set; }
    public bool IncludeInRetirementProjections { get; set; } = true;
    public string? Notes { get; set; }

    public Guid? LinkedLiabilityId { get; set; }
    public Liability? LinkedLiability { get; set; }

    public ICollection<AccountOwner> Owners { get; set; } = new List<AccountOwner>();
    public ICollection<AccountBalanceSnapshot> BalanceSnapshots { get; set; } = new List<AccountBalanceSnapshot>();
}
