using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using FinancialFreedom.Shared;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace FinancialFreedom.Client.Pages.FinancialHealth;

public partial class IncomeReplacement
{
    private const double ChartW = 880;
    private const double ChartH = 300;
    private const double PadL = 64;
    private const double PadR = 20;
    private const double PadT = 16;
    private const double PadB = 44;

    [Inject] private HttpClient Http { get; set; } = default!;

    private IncomeReplacementResultDto? _data;
    private string? _error;

    private MudBlazor.Color TrafficColor =>
        _data?.TrafficLight switch
        {
            2 => Color.Success,
            1 => Color.Warning,
            _ => Color.Error,
        };

    private string TrafficHeadline =>
        _data?.TrafficLight switch
        {
            2 => "You are on track",
            1 => "You are close — review the gap",
            _ => "There is a meaningful shortfall",
        };

    private string TrafficDetail
    {
        get
        {
            if (_data is null) return "";
            var g = _data.ReplacementGreenThresholdPercent;
            var y = _data.ReplacementYellowThresholdPercent;
            return _data.TrafficLight switch
            {
                2 => $"Estimated retirement income meets or exceeds your green threshold ({g:0.#}% of replacement salary).",
                1 => $"Income is between the yellow and green bands ({y:0.#}%–{g:0.#}% of replacement salary). Consider increasing savings or adjusting timing.",
                _ => $"Income is below the yellow threshold ({y:0.#}% of replacement salary). Strengthen contributions or revisit assumptions.",
            };
        }
    }

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _data = await Http.GetFromJsonAsync<IncomeReplacementResultDto>("api/projections/income-replacement");
        }
        catch (Exception ex)
        {
            _error = ex.Message;
        }
    }

    private static string F(double v) => v.ToString("0.##", CultureInfo.InvariantCulture);

    private static decimal NiceCeiling(decimal max)
    {
        if (max <= 0) return 1m;
        var x = (double)max * 1.06;
        var pow = Math.Pow(10, Math.Floor(Math.Log10(x)));
        var n = x / pow;
        var nice = n <= 1 ? 1 : n <= 2 ? 2 : n <= 5 ? 5 : 10;
        return (decimal)(nice * pow);
    }

    private (double iw, double ih) Inner() =>
        (ChartW - PadL - PadR, ChartH - PadT - PadB);

    private string? BalancePaths(out decimal yMax, out int minYear, out int maxYear)
    {
        yMax = 1;
        minYear = 0;
        maxYear = 0;
        if (_data?.ChartPoints is not { Count: > 0 } list)
            return null;

        var pts = list.OrderBy(p => p.Year).ToList();
        minYear = pts[0].Year;
        maxYear = pts[^1].Year;
        yMax = NiceCeiling(pts.Max(p => Math.Max(p.AllBalance, Math.Max(p.DadBalance, p.MomBalance))));

        var (iw, ih) = Inner();
        var spanYears = Math.Max(1, maxYear - minYear);
        var capY = yMax;
        var baseYear = minYear;

        string Line(Func<IncomeReplacementLinePointDto, decimal> y)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < pts.Count; i++)
            {
                var p = pts[i];
                var tx = (p.Year - baseYear) / (double)spanYears * iw;
                var vy = (double)(y(p) / capY);
                vy = Math.Clamp(vy, 0, 1);
                var ty = ih - vy * ih;
                sb.Append(i == 0 ? "M " : " L ");
                sb.Append(F(tx)).Append(',').Append(F(ty));
            }

            return sb.ToString();
        }

        return $@"<path class=""ff-ir-line ff-ir-line--dad"" d=""{Line(p => p.DadBalance)}"" fill=""none"" />
<path class=""ff-ir-line ff-ir-line--mom"" d=""{Line(p => p.MomBalance)}"" fill=""none"" />
<path class=""ff-ir-line ff-ir-line--all"" d=""{Line(p => p.AllBalance)}"" fill=""none"" />";
    }

    private string? SalaryPaths(out decimal yMax, out int minYear, out int maxYear)
    {
        yMax = 1m;
        minYear = 0;
        maxYear = 0;
        if (_data?.ChartPoints is not { Count: > 0 } list)
            return null;

        var pts = list.OrderBy(p => p.Year).ToList();
        minYear = pts[0].Year;
        maxYear = pts[^1].Year;
        yMax = NiceCeiling(pts.Max(p => Math.Max(p.CombinedSalary, Math.Max(p.DadSalary, p.MomSalary))));

        var (iw, ih) = Inner();
        var spanYears = Math.Max(1, maxYear - minYear);
        var capY = yMax;
        var baseYear = minYear;

        string Line(Func<IncomeReplacementLinePointDto, decimal> y)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < pts.Count; i++)
            {
                var p = pts[i];
                var tx = (p.Year - baseYear) / (double)spanYears * iw;
                var vy = capY > 0 ? (double)(y(p) / capY) : 0;
                vy = Math.Clamp(vy, 0, 1);
                var ty = ih - vy * ih;
                sb.Append(i == 0 ? "M " : " L ");
                sb.Append(F(tx)).Append(',').Append(F(ty));
            }

            return sb.ToString();
        }

        return $@"<path class=""ff-ir-line ff-ir-line--dad ff-ir-line--salary"" d=""{Line(p => p.DadSalary)}"" fill=""none"" />
<path class=""ff-ir-line ff-ir-line--mom ff-ir-line--salary"" d=""{Line(p => p.MomSalary)}"" fill=""none"" />
<path class=""ff-ir-line ff-ir-line--combined"" d=""{Line(p => p.CombinedSalary)}"" fill=""none"" />";
    }

    private string YAxisLabels(decimal yMax, double innerH)
    {
        const int ticks = 5;
        var sb = new StringBuilder();
        for (var i = 0; i <= ticks; i++)
        {
            var v = yMax * i / ticks;
            var y = PadT + innerH - innerH * i / ticks;
            var label = CompactMoney(v);
            sb.Append(
                $"<text class=\"ff-ir-ytick\" x=\"{F(4)}\" y=\"{F(y + 4)}\">{label}</text>");
        }

        return sb.ToString();
    }

    private string XAxisLabels(int minYear, int maxYear, double innerW, double _)
    {
        var sb = new StringBuilder();
        var span = Math.Max(1, maxYear - minYear);
        var step = span <= 10 ? 1 : span <= 20 ? 2 : 5;
        for (var y = minYear; y <= maxYear; y += step)
        {
            var tx = PadL + (y - minYear) / (double)span * innerW;
            sb.Append(
                $"<text class=\"ff-ir-xtick\" x=\"{F(tx)}\" y=\"{F(ChartH - 8)}\">{y}</text>");
        }

        return sb.ToString();
    }

    private static string CompactMoney(decimal v)
    {
        if (v >= 1_000_000m)
            return "$" + (v / 1_000_000m).ToString("0.#", CultureInfo.InvariantCulture) + "M";
        if (v >= 1000m)
            return "$" + (v / 1000m).ToString("0.#", CultureInfo.InvariantCulture) + "k";
        return "$" + v.ToString("0", CultureInfo.InvariantCulture);
    }

    private MarkupString BalanceChartSvg
    {
        get
        {
            if (_data?.ChartPoints is not { Count: > 0 })
                return new MarkupString("");

            var paths = BalancePaths(out var yMax, out var minYear, out var maxYear);
            if (paths is null)
                return new MarkupString("");

            var (iw, ih) = Inner();
            var grid = GridLines(ih);
            var yLabs = YAxisLabels(yMax, ih);
            var xLabs = XAxisLabels(minYear, maxYear, iw, ih);
            var html =
                $@"<svg class=""ff-ir-chart"" viewBox=""0 0 {F(ChartW)} {F(ChartH)}"" xmlns=""http://www.w3.org/2000/svg"" role=""img"" aria-label=""Projected retirement balances by year"">
<g transform=""translate({F(PadL)},{F(PadT)})"">
{grid}
{paths}
</g>
{yLabs}
{xLabs}
</svg>";
            return new MarkupString(html);
        }
    }

    private MarkupString SalaryChartSvg
    {
        get
        {
            if (_data?.ChartPoints is not { Count: > 0 })
                return new MarkupString("");

            var paths = SalaryPaths(out var yMax, out var minYear, out var maxYear);
            if (paths is null)
                return new MarkupString("");

            var (iw, ih) = Inner();
            var grid = GridLines(ih);
            var yLabs = YAxisLabels(yMax, ih);
            var xLabs = XAxisLabels(minYear, maxYear, iw, ih);
            var html =
                $@"<svg class=""ff-ir-chart"" viewBox=""0 0 {F(ChartW)} {F(ChartH)}"" xmlns=""http://www.w3.org/2000/svg"" role=""img"" aria-label=""Projected salaries by year while working"">
<g transform=""translate({F(PadL)},{F(PadT)})"">
{grid}
{paths}
</g>
{yLabs}
{xLabs}
</svg>";
            return new MarkupString(html);
        }
    }

    private static string GridLines(double innerH)
    {
        const int ticks = 5;
        var (iw, _) = (ChartW - PadL - PadR, innerH);
        var sb = new StringBuilder();
        for (var i = 0; i <= ticks; i++)
        {
            var y = innerH * i / ticks;
            sb.Append(
                $"<line class=\"ff-ir-grid\" x1=\"0\" y1=\"{F(y)}\" x2=\"{F(iw)}\" y2=\"{F(y)}\" />");
        }

        return sb.ToString();
    }
}
