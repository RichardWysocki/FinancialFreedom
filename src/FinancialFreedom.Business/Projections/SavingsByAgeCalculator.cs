namespace FinancialFreedom.Business.Projections;

public sealed record SavingsByAgeOutput(
    int OldestAdultAge,
    decimal SavingFactorMultiple,
    decimal CombinedSalary,
    decimal CurrentRetirementAssets,
    decimal TargetRetirementSavings,
    decimal SuccessRatePercent);

public static class SavingsByAgeCalculator
{
    /// <summary>Age thresholds → multiple of salary (floor lookup by age as of Jan 1).</summary>
    public static SavingsByAgeOutput Compute(
        int oldestAdultAge,
        decimal combinedSalary,
        decimal currentRetirementAssets,
        IReadOnlyList<(int Age, decimal Multiple)> multiplesTable)
    {
        var sorted = multiplesTable.OrderBy(x => x.Age).ToList();
        decimal factor = sorted[0].Multiple;
        foreach (var (age, mult) in sorted)
        {
            if (oldestAdultAge >= age)
                factor = mult;
            else
                break;
        }

        var target = combinedSalary * factor;
        var success = target > 0 ? currentRetirementAssets / target * 100m : 0;
        return new SavingsByAgeOutput(oldestAdultAge, factor, combinedSalary, currentRetirementAssets, target, success);
    }
}
