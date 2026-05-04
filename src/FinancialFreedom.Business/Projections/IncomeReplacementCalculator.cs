namespace FinancialFreedom.Business.Projections;

public static class IncomeReplacementCalculator
{
    public static IncomeReplacementOutput Compute(HouseholdProjectionInput input, IrsLimitLookup irs)
    {
        var adults = input.Adults
            .Where(a => a.RetirementAge > a.CurrentAge && a.CurrentSalary > 0)
            .ToList();

        if (adults.Count == 0)
        {
            return new IncomeReplacementOutput(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, Array.Empty<YearBalancePoint>());
        }

        var oldest = adults.OrderByDescending(a => a.CurrentAge).First();
        var yearsToSimulate = Math.Max(1, oldest.RetirementAge - oldest.CurrentAge);
        var startYear = oldest.CurrentYear;
        var endYear = startYear + yearsToSimulate - 1;

        // Future salary at retirement year (sum of each adult's projected salary in their retirement year).
        decimal futureSalarySum = 0;
        foreach (var a in adults)
        {
            var years = Math.Max(0, a.RetirementAge - a.CurrentAge);
            var sal = a.CurrentSalary;
            for (var i = 0; i < years; i++)
                sal *= 1 + a.SalaryIncreasePercentPerYear / 100m;
            futureSalarySum += sal;
        }

        // First-year contributions (for replacement salary line — matches common "current plan" reading).
        decimal annualRetirementContrib = 0;
        decimal annualHsaContrib = 0;
        foreach (var a in adults)
        {
            annualRetirementContrib += a.CurrentSalary * (a.ContributionPercentOfSalary / 100m);
            annualHsaContrib += a.AnnualHsaContribution;
        }

        var replacementSalary = futureSalarySum - annualRetirementContrib - annualHsaContrib;

        var retirementBalances = adults.ToDictionary(a => a.Name, a => a.RetirementBalance, StringComparer.OrdinalIgnoreCase);
        var hsaBalances = adults.ToDictionary(a => a.Name, a => a.HsaBalance, StringComparer.OrdinalIgnoreCase);

        var rr = input.RetirementReturnPercentPerYear / 100m;
        var hr = input.HsaReturnPercentPerYear / 100m;

        var chartPoints = new List<YearBalancePoint>();

        for (var year = startYear; year <= endYear; year++)
        {
            var yearIndex = year - startYear;
            decimal allRetirement = 0;

            foreach (var a in adults)
            {
                var age = a.CurrentAge + yearIndex;
                var rb = retirementBalances[a.Name];
                rb *= 1 + rr;

                if (age < a.RetirementAge)
                {
                    var salary = a.CurrentSalary;
                    for (var i = 0; i < yearIndex; i++)
                        salary *= 1 + a.SalaryIncreasePercentPerYear / 100m;

                    var employee = salary * (a.ContributionPercentOfSalary / 100m);
                    var capSalary = salary * (a.CompanyMatchCapPercentOfSalary / 100m);
                    var matchable = Math.Min(employee, capSalary);
                    var employer = matchable * (a.CompanyMatchPercentOfEmployeeContribution / 100m);

                    var deferralLimit = irs.Get401kElectiveDeferral(year);
                    if (a.UseCatchUp50 && age >= 50)
                        deferralLimit += irs.Get401kCatchUp50(year);

                    employee = Math.Min(employee, deferralLimit);
                    rb += employee + employer;
                }

                retirementBalances[a.Name] = rb;
                allRetirement += rb;

                var hb = hsaBalances[a.Name];
                hb *= 1 + hr;
                if (age < a.RetirementAge && a.AnnualHsaContribution > 0)
                    hb += a.AnnualHsaContribution;
                hsaBalances[a.Name] = hb;

                chartPoints.Add(new YearBalancePoint(year, a.Name, rb));
            }

            chartPoints.Add(new YearBalancePoint(year, "All", allRetirement));
        }

        var oldestRetirementAge = adults.Min(a => a.RetirementAge);
        var futureRetirement = adults.Sum(a => retirementBalances[a.Name]);
        var futureHsa = adults.Sum(a => hsaBalances[a.Name]);

        var withdrawAnnual = futureRetirement * (input.WithdrawalRatePercent / 100m);
        var retirementYears = Math.Max(1, input.LifeExpectancyAge - oldestRetirementAge);
        var hsaWithdrawAnnual = futureHsa / retirementYears;

        var ssAnnual = adults.Sum(a => a.SocialSecurityMonthlyBenefit * 12m);
        var totalIncome = withdrawAnnual + hsaWithdrawAnnual + ssAnnual;
        var replacementPct = replacementSalary > 0 ? totalIncome / replacementSalary * 100m : 0;

        var traffic =
            replacementPct >= input.ReplacementGreenPercent ? 2 :
            replacementPct >= input.ReplacementYellowPercent ? 1 : 0;

        return new IncomeReplacementOutput(
            futureSalarySum,
            annualRetirementContrib,
            annualHsaContrib,
            replacementSalary,
            futureRetirement,
            withdrawAnnual,
            futureHsa,
            hsaWithdrawAnnual,
            ssAnnual,
            totalIncome,
            replacementPct,
            traffic,
            chartPoints);
    }
}
