namespace FinancialFreedom.Server.Data;

public enum FamilyMemberType
{
    Adult = 0,
    Kid = 1,
}

public enum AssetClass
{
    Liquid = 0,
    SemiLiquid = 1,
}

public enum TaxTreatment
{
    PreTax = 0,
    Roth = 1,
    Taxable = 2,
    Hsa = 3,
    Education = 4,
}

public enum LiabilityType
{
    Mortgage = 0,
    Loan = 1,
    CreditCard = 2,
    Other = 3,
}

public enum SnapshotSource
{
    Manual = 0,
    Imported = 1,
    SaveData = 2,
}

public enum IrsLimitType
{
    HsaSingle = 0,
    HsaFamily = 1,
    HsaCatchup55 = 2,
    Plan401kElectiveDeferral = 3,
    Plan401kCatchup50 = 4,
    IraContribution = 5,
    IraCatchup50 = 6,
    SocialSecurityWageBase = 7,
}
