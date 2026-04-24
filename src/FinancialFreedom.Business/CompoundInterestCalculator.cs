namespace FinancialFreedom.Business;

/// <summary>
/// Pure projection math for savings or portfolio growth (annual compounding).
/// </summary>
public static class CompoundInterestCalculator
{
    public static decimal FutureValue(decimal principal, decimal annualRate, int years)
    {
        if (years < 0)
            throw new ArgumentOutOfRangeException(nameof(years));

        if (principal < 0)
            throw new ArgumentOutOfRangeException(nameof(principal));

        if (annualRate < -1m || annualRate > 100m)
            throw new ArgumentOutOfRangeException(nameof(annualRate));

        var factor = (double)(1m + annualRate);
        return principal * (decimal)Math.Pow(factor, years);
    }
}
