namespace FinancialFreedom.Shared;

public class FutureSalaryProjectionRequest
{
    public int CurrentAge { get; set; }
    public int RetirementAge { get; set; }
    public decimal CurrentSalary { get; set; }
    public decimal AnnualIncreasePercent { get; set; }
    /// <summary>If null, the server uses the current UTC calendar year when building the projection.</summary>
    public int? CalendarStartYear { get; set; }
}

public class FutureSalaryProjectionResponse
{
    public List<FutureSalaryProjectionRow> Rows { get; set; } = [];
    public int CalendarStartYear { get; set; }
    public int CalendarEndYear { get; set; }
    public int EstimatedRetirementYear { get; set; }
}

public class FutureSalaryProjectionRow
{
    public int Age { get; set; }
    public int CalendarYear { get; set; }
    public decimal Salary { get; set; }
}

public class ApiErrorResponse
{
    public string? Message { get; set; }
}
