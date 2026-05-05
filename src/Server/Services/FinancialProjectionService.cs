using FinancialFreedom.Business.Projections;
using FinancialFreedom.Server.Data;
using FinancialFreedom.Shared;
using Microsoft.EntityFrameworkCore;

namespace FinancialFreedom.Server.Services;

public interface IFinancialProjectionService
{
    Task<SavingsByAgeResultDto?> GetSavingsByAgeAsync(Guid householdId, CancellationToken cancellationToken = default);
    Task<IncomeReplacementResultDto?> GetIncomeReplacementAsync(Guid householdId, CancellationToken cancellationToken = default);
}

public class FinancialProjectionService(AppDbContext db) : IFinancialProjectionService
{
    public async Task<SavingsByAgeResultDto?> GetSavingsByAgeAsync(Guid householdId, CancellationToken cancellationToken = default)
    {
        var members = await db.FamilyMembers.AsNoTracking()
            .Where(m => m.HouseholdId == householdId)
            .ToListAsync(cancellationToken);

        var adults = members.Where(m => m.Type == FamilyMemberType.Adult).ToList();
        if (adults.Count == 0)
            return null;

        var jan1 = new DateOnly(DateTime.UtcNow.Year, 1, 1);
        var oldestAge = adults.Max(a => AgeOnDate(a.DateOfBirth, jan1));

        var salaries = await db.RetirementProfiles.AsNoTracking()
            .Where(r => adults.Select(x => x.Id).Contains(r.FamilyMemberId))
            .SumAsync(r => r.Salary, cancellationToken);

        var retirementCategoryIds = await db.AssetCategories.AsNoTracking()
            .Where(c => c.HouseholdId == householdId && c.IsRetirement)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        var retirementAssets = await db.Accounts.AsNoTracking()
            .Where(a => a.HouseholdId == householdId && retirementCategoryIds.Contains(a.AssetCategoryId))
            .SumAsync(a => a.CurrentBalance, cancellationToken);

        var table = await db.SavingsByAgeMultiples.AsNoTracking()
            .OrderBy(x => x.Age)
            .Select(x => new ValueTuple<int, decimal>(x.Age, x.Multiple))
            .ToListAsync(cancellationToken);

        var result = SavingsByAgeCalculator.Compute(
            oldestAge,
            salaries,
            retirementAssets,
            table.Select(t => (t.Item1, t.Item2)).ToList());

        return new SavingsByAgeResultDto(
            result.OldestAdultAge,
            result.SavingFactorMultiple,
            result.CombinedSalary,
            result.CurrentRetirementAssets,
            result.TargetRetirementSavings,
            Math.Round(result.SuccessRatePercent, 1));
    }

    public async Task<IncomeReplacementResultDto?> GetIncomeReplacementAsync(Guid householdId, CancellationToken cancellationToken = default)
    {
        var members = await db.FamilyMembers.AsNoTracking()
            .Include(m => m.RetirementProfile)
            .Include(m => m.HsaProfile)
            .Where(m => m.HouseholdId == householdId && m.Type == FamilyMemberType.Adult)
            .ToListAsync(cancellationToken);

        var adultsWithSalary = members.Where(m => m.RetirementProfile is not null && m.RetirementProfile.Salary > 0).ToList();
        if (adultsWithSalary.Count == 0)
            return null;

        var assumptions = await db.ProjectionAssumptions.AsNoTracking()
            .FirstOrDefaultAsync(p => p.HouseholdId == householdId, cancellationToken)
            ?? new ProjectionAssumptions();

        var accounts = await db.Accounts.AsNoTracking()
            .Include(a => a.AssetCategory)
            .Include(a => a.Owners)
            .Where(a => a.HouseholdId == householdId)
            .ToListAsync(cancellationToken);

        var irsRows = await db.IrsLimits.AsNoTracking().ToListAsync(cancellationToken);
        var irsDict = irsRows.ToDictionary(
            r => (r.Year, (IrsLimitKind)(int)r.LimitType),
            r => r.Amount);
        var irs = new IrsLimitLookup(irsDict);

        var currentYear = DateTime.UtcNow.Year;
        var jan1 = new DateOnly(currentYear, 1, 1);

        var adultInputs = new List<AdultProjectionInput>();
        foreach (var m in adultsWithSalary)
        {
            var rp = m.RetirementProfile!;
            var hp = m.HsaProfile;
            var age = AgeOnDate(m.DateOfBirth, jan1);

            var retBal = SumMemberBalance(accounts, a => a.AssetCategory.IsRetirement, m.Id);
            var hsaBal = SumMemberBalance(accounts, a => a.AssetCategory.IsHsa, m.Id);

            adultInputs.Add(new AdultProjectionInput(
                m.Name,
                age,
                rp.RetirementAge,
                rp.Salary,
                rp.EstimatedSalaryIncreasePercent,
                retBal,
                hsaBal,
                rp.ContributionPercent,
                rp.HasRetirementCatchup,
                rp.CompanyMatchPercent,
                rp.CompanyMatchEndsAtSalaryPercent,
                rp.SocialSecurityMonthlyBenefit,
                hp?.AnnualHsaContribution ?? 0,
                hp?.HasHsaCatchup ?? false,
                currentYear));
        }

        if (adultInputs.TrueForAll(a => a.RetirementBalance == 0))
        {
            var totalRet = accounts.Where(a => a.AssetCategory.IsRetirement).Sum(a => a.CurrentBalance);
            var share = adultInputs.Count > 0 ? totalRet / adultInputs.Count : 0;
            adultInputs = adultInputs.Select(a => a with { RetirementBalance = share }).ToList();
        }

        if (adultInputs.TrueForAll(a => a.HsaBalance == 0))
        {
            var totalHsa = accounts.Where(a => a.AssetCategory.IsHsa).Sum(a => a.CurrentBalance);
            var share = adultInputs.Count > 0 ? totalHsa / adultInputs.Count : 0;
            adultInputs = adultInputs.Select(a => a with { HsaBalance = share }).ToList();
        }

        var hpInput = new HouseholdProjectionInput(
            adultInputs,
            assumptions.RetirementRateOfReturnPercent,
            assumptions.HsaRateOfReturnPercent,
            assumptions.RetirementWithdrawalRatePercent,
            assumptions.LifeExpectancyAge,
            assumptions.ReplacementRateGreenThresholdPercent,
            assumptions.ReplacementRateYellowThresholdPercent,
            currentYear + 40);

        var output = IncomeReplacementCalculator.Compute(hpInput, irs);

        var byYear = output.ChartPoints.GroupBy(p => p.Year).OrderBy(g => g.Key);
        var salaryByYear = output.SalaryPoints.GroupBy(p => p.Year).ToDictionary(g => g.Key, g => g.ToList());
        var linePoints = new List<IncomeReplacementLinePointDto>();
        foreach (var g in byYear)
        {
            decimal dad = g.FirstOrDefault(x => x.SeriesName.Equals("Dad", StringComparison.OrdinalIgnoreCase))?.Balance ?? 0;
            decimal mom = g.FirstOrDefault(x => x.SeriesName.Equals("Mom", StringComparison.OrdinalIgnoreCase))?.Balance ?? 0;
            var all = g.FirstOrDefault(x => x.SeriesName == "All")?.Balance ?? dad + mom;
            var salaries = salaryByYear.GetValueOrDefault(g.Key, []);
            var dadSal = salaries.FirstOrDefault(x => x.Name.Equals("Dad", StringComparison.OrdinalIgnoreCase))?.AnnualSalary ?? 0;
            var momSal = salaries.FirstOrDefault(x => x.Name.Equals("Mom", StringComparison.OrdinalIgnoreCase))?.AnnualSalary ?? 0;
            var combinedSal = salaries.Sum(x => x.AnnualSalary);
            linePoints.Add(new IncomeReplacementLinePointDto(g.Key, dad, mom, all, dadSal, momSal, combinedSal));
        }

        return new IncomeReplacementResultDto(
            Math.Round(output.EstimatedFutureFamilySalary, 2),
            Math.Round(output.AnnualRetirementContributions, 2),
            Math.Round(output.AnnualHealthSavingsContributions, 2),
            Math.Round(output.EstimatedReplacementAnnualSalary, 2),
            Math.Round(output.FutureRetirementSavingsApprox, 2),
            Math.Round(output.RetirementWithdrawAnnual, 2),
            Math.Round(output.FutureHealthSavingsApprox, 2),
            Math.Round(output.HealthWithdrawAnnual, 2),
            Math.Round(output.EstimatedSocialSecurityAnnual, 2),
            Math.Round(output.EstimatedTotalRetirementIncome, 2),
            Math.Round(output.ReplacementPercent, 1),
            output.TrafficLight,
            Math.Round(assumptions.ReplacementRateGreenThresholdPercent, 2),
            Math.Round(assumptions.ReplacementRateYellowThresholdPercent, 2),
            Math.Round(assumptions.RetirementRateOfReturnPercent, 2),
            Math.Round(assumptions.HsaRateOfReturnPercent, 2),
            Math.Round(assumptions.InflationRatePercent, 2),
            linePoints);
    }

    private static decimal SumMemberBalance(
        List<Account> accounts,
        Func<Account, bool> categoryPredicate,
        Guid memberId)
    {
        decimal sum = 0;
        foreach (var a in accounts.Where(categoryPredicate))
        {
            var owners = a.Owners.ToList();
            if (owners.Count == 0)
                continue;

            var pct = owners.Where(o => o.FamilyMemberId == memberId).Sum(o => o.OwnershipPercent);
            if (pct > 0)
                sum += a.CurrentBalance * (pct / 100m);
        }

        return sum;
    }

    private static int AgeOnDate(DateOnly dob, DateOnly onDate)
    {
        var age = onDate.Year - dob.Year;
        var birthdayThisYear = new DateOnly(onDate.Year, dob.Month, SafeDay(onDate.Year, dob.Month, dob.Day));
        if (onDate < birthdayThisYear)
            age--;
        return Math.Max(0, age);
    }

    private static int SafeDay(int year, int month, int day)
    {
        var dim = DateTime.DaysInMonth(year, month);
        return Math.Min(day, dim);
    }
}
