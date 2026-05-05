using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using FinancialFreedom.Shared;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace FinancialFreedom.Client.Pages.FinancialHealth;

public partial class SavingsByAge
{
    private const double GaugeCx = 120;
    private const double GaugeCy = 118;
    private const double GaugeR = 92;

    private static readonly SavingsByAgeMultipleDto[] DefaultLadder =
    [
        new(30, 1m), new(35, 2m), new(40, 3m), new(45, 4m),
        new(50, 6m), new(55, 7m), new(60, 8m), new(67, 10m),
    ];

    [Inject] private HttpClient Http { get; set; } = default!;

    private SavingsByAgeResultDto? _result;
    private IReadOnlyList<SavingsByAgeMultipleDto> _ladder = DefaultLadder;
    private string? _error;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _result = await Http.GetFromJsonAsync<SavingsByAgeResultDto>("api/projections/savings-by-age");
        }
        catch (Exception ex)
        {
            _error = ex.Message;
        }

        try
        {
            var ladder = await Http.GetFromJsonAsync<List<SavingsByAgeMultipleDto>>("api/reference/savings-by-age");
            if (ladder is { Count: > 0 })
                _ladder = ladder.OrderBy(x => x.Age).ToList();
        }
        catch
        {
            /* Milestones still render from DefaultLadder */
        }
    }

    /// <summary>Needle and arc scale: 0–100 on the gauge; values over 100 pin to the top of the scale.</summary>
    private static double GaugeNeedlePercent(decimal successRatePercent) =>
        Math.Clamp((double)successRatePercent, 0, 100);

    private static string F(double v) => v.ToString("0.###", CultureInfo.InvariantCulture);

    /// <summary>Semicircle opening upward: 0% = left, 100% = right. Center (cx,cy), radius r.</summary>
    private static (double x, double y) GaugePoint(double cx, double cy, double r, double percent)
    {
        var theta = Math.PI * (1 - percent / 100.0);
        return (cx + r * Math.Cos(theta), cy - r * Math.Sin(theta));
    }

    private static string GaugeArcPath(double cx, double cy, double r, double fromPct, double toPct)
    {
        var (x1, y1) = GaugePoint(cx, cy, r, fromPct);
        var (x2, y2) = GaugePoint(cx, cy, r, toPct);
        return $"M {F(x1)},{F(y1)} A {F(r)},{F(r)} 0 0 1 {F(x2)},{F(y2)}";
    }

    private static string NeedleLine(double cx, double cy, double r, double percent)
    {
        var p = GaugeNeedlePercent((decimal)percent);
        var (x, y) = GaugePoint(cx, cy, r - 10, p);
        return $"M {F(cx)},{F(cy)} L {F(x)},{F(y)}";
    }

    private Color SuccessMudColor(decimal success) =>
        success >= 100 ? Color.Success :
        success >= 60 ? Color.Info :
        success >= 30 ? Color.Warning :
        Color.Error;

    private string SuccessLabel(decimal success) =>
        success >= 100
            ? "You are at or above the guideline for your age group."
            : success >= 60
                ? "You are close to the guideline — keep building retirement savings."
                : success >= 30
                    ? "There is a gap versus this guideline — consider increasing savings rate."
                    : "This guideline suggests a meaningful savings gap — review contributions and assumptions.";

    private static string FormatFactor(decimal m) => m.ToString("0.##", CultureInfo.InvariantCulture);
}
