namespace FinancialFreedom.Client.Pages;

public partial class FinancialWellness
{
    private static string SectionId(int index) => $"section-{index}";

    private readonly WellnessSection[] _sections =
    [
        new("Dinkytown — calculators", [
            new WellnessLink("Inflation and Consumer Prices Calculator", "https://www.dinkytown.net/java/inflation-and-consumer-prices-calculator.html"),
            new WellnessLink("How long will my retirement savings last?", "https://www.dinkytown.net/java/how-long-will-my-retirement-savings-last.html"),
            new WellnessLink("Inflation — Historic impact on investments", "https://www.dinkytown.net/java/inflation-historic-impact-on-investments.html"),
            new WellnessLink("401(k) calculator", "https://www.dinkytown.net/java/401k-calculator.html"),
            new WellnessLink("All Dinkytown calculators (index)", "https://www.dinkytown.net/java/", "Browse the full Java/HTML5 library"),
        ]),
        new("FNCalculators & Calculator.net", [
            new WellnessLink("FNCalculator.net — hub", "https://fncalculator.net/", "Free calculators across loans, retirement, tax, and more"),
            new WellnessLink("FNCalculator.com — hub", "https://www.fncalculator.com/", "Alternate FN calculators site"),
            new WellnessLink("Calculator.net — Bond calculator", "https://www.calculator.net/bond-calculator.html"),
        ]),
        new("Boldin (retirement planning)", [
            new WellnessLink("Boldin — home", "https://www.boldin.com/"),
            new WellnessLink("Retirement savings calculator (simple)", "https://boldin.com/retirement/simple-retirement-calculator", "Quick “enough for retirement?” check"),
        ]),
        new("Coast FIRE", [
            new WellnessLink("Coast FIRE calculator — WalletBurst", "https://www.walletburst.com/tools/coast-fire-calc/"),
            new WellnessLink("Coast FIRE calculator — Coast FIRE Calculator", "https://coastfire-calculator.com/", "Alternative Coast FIRE planner"),
        ]),
        new("Planning & simulation", [
            new WellnessLink("Honest Math — app", "https://app.honestmath.com/", "Monte Carlo retirement simulation"),
            new WellnessLink("Honest Math — site", "https://www.honestmath.com/", "Guides and context"),
            new WellnessLink("Retirement planner — accumulation & distribution", "https://accuratecalculators.com/retirement-calculator", "Schedule-style retirement planner"),
        ]),
        new("Funds, basis & Social Security", [
            new WellnessLink("FINRA Fund Analyzer", "https://tools.finra.org/fund_analyzer/", "Fees and fund comparison"),
            new WellnessLink("Understanding cost basis (Investopedia)", "https://www.investopedia.com/terms/c/costbasis.asp", "Calculation, examples, tax impact"),
            new WellnessLink("How Social Security benefits are computed: In Brief (CRS)", "https://www.congress.gov/crs-product/R43542", "Congressional Research Service overview"),
            new WellnessLink("SSA — computing a retired-worker benefit", "https://www.ssa.gov/policy/docs/statcomps/supplement/2024/apnc.html", "Official SSA methodology reference"),
        ]),
        new("Reading & tools lists", [
            new WellnessLink("Rob Berger — Tools archive", "https://robberger.com/category/tools/"),
        ]),
        new("Salary projections", [
            new WellnessLink("Future salary calculator and table — EasySurf", "https://www.easysurf.cc/fsalary.htm", "Year-by-year salary table from raises"),
        ]),
    ];

    private sealed record WellnessLink(string Label, string Url, string? Hint = null);

    private sealed record WellnessSection(string Title, WellnessLink[] Links);
}
