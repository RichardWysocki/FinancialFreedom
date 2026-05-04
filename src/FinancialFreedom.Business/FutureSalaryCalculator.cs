using FinancialFreedom.Shared;

namespace FinancialFreedom.Business;

public static class FutureSalaryCalculator
{
    public const int MaxProjectionYears = 80;
    public const int MaxAge = 120;

    /// <summary>
    /// Builds a year-by-year salary projection from current age through retirement age.
    /// </summary>
    /// <param name="request">Inputs from the client.</param>
    /// <param name="calendarStartYear">First calendar year in the table (typically current year).</param>
    /// <param name="errorMessage">Set when the return value is null.</param>
    /// <returns>Projection payload, or null when validation fails.</returns>
    public static FutureSalaryProjectionResponse? TryProject(
        FutureSalaryProjectionRequest request,
        int calendarStartYear,
        out string? errorMessage)
    {
        errorMessage = null;

        if (request.CurrentAge < 0 || request.CurrentAge > MaxAge)
        {
            errorMessage = "Enter a realistic current age.";
            return null;
        }

        if (request.RetirementAge <= request.CurrentAge)
        {
            errorMessage = "Retirement age must be greater than current age.";
            return null;
        }

        var yearsSpan = request.RetirementAge - request.CurrentAge;
        if (yearsSpan > MaxProjectionYears)
        {
            errorMessage = $"Projection is limited to {MaxProjectionYears} years. Choose a nearer retirement age.";
            return null;
        }

        if (request.CurrentSalary <= 0)
        {
            errorMessage = "Salary must be greater than zero.";
            return null;
        }

        if (request.AnnualIncreasePercent < 0 || request.AnnualIncreasePercent > 100)
        {
            errorMessage = "Annual increase should be between 0% and 100%.";
            return null;
        }

        if (calendarStartYear < 1 || calendarStartYear > 9999)
        {
            errorMessage = "Calendar start year is not valid.";
            return null;
        }

        var endYear = calendarStartYear + yearsSpan;
        var factor = 1m + request.AnnualIncreasePercent / 100m;
        var rows = new List<FutureSalaryProjectionRow>(yearsSpan + 1);

        for (var n = 0; n <= yearsSpan; n++)
        {
            rows.Add(new FutureSalaryProjectionRow
            {
                Age = request.CurrentAge + n,
                CalendarYear = calendarStartYear + n,
                Salary = request.CurrentSalary * PowDecimal(factor, n),
            });
        }

        errorMessage = null;
        return new FutureSalaryProjectionResponse
        {
            Rows = rows,
            CalendarStartYear = calendarStartYear,
            CalendarEndYear = endYear,
            EstimatedRetirementYear = calendarStartYear + yearsSpan,
        };
    }

    private static decimal PowDecimal(decimal bas, int exp)
    {
        if (exp <= 0) return 1m;
        var r = 1m;
        for (var i = 0; i < exp; i++) r *= bas;
        return r;
    }
}
