using FinancialFreedom.Business;

namespace FinancialFreedom.UnitTests;

public class CompoundInterestCalculatorTests
{
    [Fact]
    public void FutureValue_10_percent_over_two_years_matches_expected()
    {
        var result = CompoundInterestCalculator.FutureValue(1000m, 0.1m, 2);
        Assert.Equal(1210m, result);
    }

    [Fact]
    public void FutureValue_zero_years_returns_principal()
    {
        var result = CompoundInterestCalculator.FutureValue(500m, 0.05m, 0);
        Assert.Equal(500m, result);
    }

    [Fact]
    public void FutureValue_rejects_negative_years()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CompoundInterestCalculator.FutureValue(1m, 0.01m, -1));
    }
}
