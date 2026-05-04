namespace FinancialFreedom.Client.Identity.Models;

public sealed class FormResult
{
    public bool Succeeded { get; init; }
    public IReadOnlyList<string> ErrorList { get; init; } = Array.Empty<string>();
}
