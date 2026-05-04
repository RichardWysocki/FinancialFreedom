using Microsoft.AspNetCore.Components;

namespace FinancialFreedom.Client.Shared;

public partial class RedirectToLogin
{
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    protected override void OnInitialized()
    {
        Navigation.NavigateTo("login", forceLoad: false);
    }
}
