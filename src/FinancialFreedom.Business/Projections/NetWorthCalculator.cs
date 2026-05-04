namespace FinancialFreedom.Business.Projections;

public sealed record AccountBalanceInput(
    Guid AccountId,
    string AccountName,
    string CategoryName,
    int AssetClass,
    int TaxTreatment,
    decimal Balance,
    IReadOnlyList<(Guid MemberId, string MemberName, decimal Pct)> Owners);

public sealed record NetWorthBreakdown(
    decimal TotalLiquidAssets,
    decimal SemiLiquidNetEquity,
    decimal TotalLiabilities,
    decimal TotalGoals,
    decimal NetWorthLiquid,
    decimal NetWorthIncludingRealEstate);

public static class NetWorthCalculator
{
    public static NetWorthBreakdown Compute(
        IReadOnlyList<AccountBalanceInput> accounts,
        decimal liabilitiesTotal,
        decimal goalsTotal)
    {
        decimal liquid = 0;
        decimal semiNet = 0;

        foreach (var a in accounts)
        {
            if (a.AssetClass == 1) // SemiLiquid
                semiNet += a.Balance;
            else
                liquid += a.Balance;
        }

        var nwLiquid = liquid - liabilitiesTotal - goalsTotal;
        var nwTotal = liquid + semiNet - liabilitiesTotal - goalsTotal;

        return new NetWorthBreakdown(liquid, semiNet, liabilitiesTotal, goalsTotal, nwLiquid, nwTotal);
    }

    public static decimal FilterByOwner(
        IReadOnlyList<AccountBalanceInput> accounts,
        Guid? memberId,
        bool jointOnly)
    {
        if (jointOnly)
            return accounts.Where(a => a.Owners.Count > 1).Sum(a => a.Balance);

        if (memberId is null)
            return accounts.Sum(a => a.Balance);

        return accounts
            .Where(a => a.Owners.Any(o => o.MemberId == memberId))
            .Sum(a =>
            {
                var pct = a.Owners.Where(o => o.MemberId == memberId).Sum(o => o.Pct);
                return a.Balance * (pct / 100m);
            });
    }
}
