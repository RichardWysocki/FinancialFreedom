namespace FinancialFreedom.Server.Data;

public class PageVisitLog
{
    public Guid Id { get; set; }
    public DateTimeOffset VisitedAt { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string PagePath { get; set; } = string.Empty;
    public string? QueryString { get; set; }
}
