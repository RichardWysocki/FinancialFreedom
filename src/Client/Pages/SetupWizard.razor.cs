using Microsoft.AspNetCore.Components;

namespace FinancialFreedom.Client.Pages;

public partial class SetupWizard
{
    [Inject] private NavigationManager Nav { get; set; } = default!;

    private int _step;
}
