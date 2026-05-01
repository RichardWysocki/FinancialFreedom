using FinancialFreedom.Business;
using FinancialFreedom.Shared;

namespace FinancialFreedom.UnitTests;

public class FutureSalaryCalculatorTests
{
    [Fact]
    public void TryProject_three_percent_for_two_years_matches_compounded_salary()
    {
        var request = new FutureSalaryProjectionRequest
        {
            CurrentAge = 40,
            RetirementAge = 42,
            CurrentSalary = 100_000m,
            AnnualIncreasePercent = 3m,
        };

        var result = FutureSalaryCalculator.TryProject(request, 2026, out var error);
        Assert.Null(error);
        Assert.NotNull(result);
        Assert.Equal(3, result!.Rows.Count);
        Assert.Equal(40, result.Rows[0].Age);
        Assert.Equal(2026, result.Rows[0].CalendarYear);
        Assert.Equal(100_000m, result.Rows[0].Salary);
        Assert.Equal(41, result.Rows[1].Age);
        Assert.Equal(103_000m, result.Rows[1].Salary);
        Assert.Equal(42, result.Rows[2].Age);
        Assert.Equal(106_090m, result.Rows[2].Salary);
        Assert.Equal(2028, result.CalendarEndYear);
        Assert.Equal(2028, result.EstimatedRetirementYear);
    }

    [Fact]
    public void TryProject_rejects_retirement_age_not_greater_than_current()
    {
        var request = new FutureSalaryProjectionRequest
        {
            CurrentAge = 50,
            RetirementAge = 50,
            CurrentSalary = 1m,
            AnnualIncreasePercent = 0m,
        };

        var result = FutureSalaryCalculator.TryProject(request, 2026, out var error);
        Assert.Null(result);
        Assert.Contains("greater than current", error, StringComparison.OrdinalIgnoreCase);
    }
}
