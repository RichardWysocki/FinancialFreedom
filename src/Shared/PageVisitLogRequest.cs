namespace FinancialFreedom.Shared;

/// <summary>Client payload for recording a Blazor route view (server fills user from the auth cookie when present).</summary>
public class PageVisitLogRequest
{
    /// <summary>Path portion only, e.g. /wellness or /future-salary</summary>
    public string PagePath { get; set; } = string.Empty;

    /// <summary>Query string without leading ?, if any.</summary>
    public string? QueryString { get; set; }
}
