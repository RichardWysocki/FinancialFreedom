namespace FinancialFreedom.Business.Projections;

public enum IrsLimitKind
{
    Plan401kElectiveDeferral = 3,
    Plan401kCatchup50 = 4,
    HsaSingle = 0,
    HsaFamily = 1,
    HsaCatchup55 = 2,
}

/// <summary>Year → limit lookup built from DB seed data.</summary>
public sealed class IrsLimitLookup(IReadOnlyDictionary<(int Year, IrsLimitKind Kind), decimal> limits)
{
    public decimal Get401kElectiveDeferral(int year) =>
        limits.TryGetValue((year, IrsLimitKind.Plan401kElectiveDeferral), out var v) ? v : 23_000m;

    public decimal Get401kCatchUp50(int year) =>
        limits.TryGetValue((year, IrsLimitKind.Plan401kCatchup50), out var v) ? v : 7_500m;
}
