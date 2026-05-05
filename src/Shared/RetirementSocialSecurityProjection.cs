namespace FinancialFreedom.Shared;

/// <summary>
/// Projects Social Security monthly benefit (entered in today's dollars) to an estimated
/// monthly amount at retirement using a constant annual inflation rate.
/// </summary>
public static class RetirementSocialSecurityProjection
{
    /// <summary>Completed age on <paramref name="asOf"/> (birthday not yet reached this year => subtract one).</summary>
    public static int AgeCompletedYears(DateOnly dateOfBirth, DateOnly asOf)
    {
        var age = asOf.Year - dateOfBirth.Year;
        if (asOf < dateOfBirth.AddYears(age))
            age--;
        return age;
    }

    /// <summary>
    /// Estimated monthly benefit at retirement (future dollars), compounded annually by <paramref name="annualInflationPercent"/>.
    /// </summary>
    /// <param name="asOfDate">Reference date for age / years-to-retirement (caller should use consistent clock, e.g. UTC server vs local browser).</param>
    public static decimal ProjectedMonthly(
        decimal monthlyTodayDollars,
        decimal annualInflationPercent,
        DateOnly dateOfBirth,
        int retirementAge,
        DateOnly asOfDate)
    {
        var age = AgeCompletedYears(dateOfBirth, asOfDate);
        var years = Math.Max(0, retirementAge - age);
        if (years == 0)
            return decimal.Round(monthlyTodayDollars, 2, MidpointRounding.AwayFromZero);

        var r = (double)(1m + annualInflationPercent / 100m);
        var factor = (decimal)Math.Pow(r, years);
        return decimal.Round(monthlyTodayDollars * factor, 2, MidpointRounding.AwayFromZero);
    }
}
