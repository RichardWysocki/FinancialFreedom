namespace FinancialFreedom.Server.Data;

/// <summary>Fidelity-style retirement savings multiples by age threshold.</summary>
public class SavingsByAgeMultiple
{
    public int Id { get; set; }
    public int Age { get; set; }
    public decimal Multiple { get; set; }
}
