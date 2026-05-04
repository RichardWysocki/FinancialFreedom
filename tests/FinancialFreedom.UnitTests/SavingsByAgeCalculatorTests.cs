using FinancialFreedom.Business.Projections;

namespace FinancialFreedom.UnitTests;

public class SavingsByAgeCalculatorTests
{
    [Fact]
    public void Age52_Uses6xFactor_And_ComputesSuccessRate()
    {
        var table = new List<(int Age, decimal Multiple)>
        {
            (30, 1), (35, 2), (40, 3), (45, 4), (50, 6), (55, 7), (60, 8), (67, 10),
        };

        var r = SavingsByAgeCalculator.Compute(52, 272_119m, 1_349_553.89m, table);

        Assert.Equal(6m, r.SavingFactorMultiple);
        Assert.Equal(272_119m * 6m, r.TargetRetirementSavings);
        Assert.InRange((double)r.SuccessRatePercent, 82.0, 83.5);
    }
}
