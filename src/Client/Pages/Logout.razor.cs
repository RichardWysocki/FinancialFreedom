using System.Threading.Tasks;
using FinancialFreedom.Client.Identity;
using Microsoft.AspNetCore.Components;

namespace FinancialFreedom.Client.Pages;

public partial class Logout
{
    [Inject] private IAccountManagement Account { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        await Account.LogoutAsync();
        Navigation.NavigateTo("", forceLoad: true);
    }
}
