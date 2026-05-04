using System.Threading.Tasks;
using FinancialFreedom.Client.Identity;
using Microsoft.AspNetCore.Components;

namespace FinancialFreedom.Client.Pages;

public partial class Login
{
    [Inject] private IAccountManagement Account { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private readonly LoginModel _model = new();
    private string? _error;
    private bool _busy;

    private async Task SubmitAsync()
    {
        _error = null;
        if (string.IsNullOrWhiteSpace(_model.Email) || string.IsNullOrWhiteSpace(_model.Password))
        {
            _error = "Enter email and password.";
            return;
        }

        _busy = true;
        try
        {
            var result = await Account.LoginAsync(_model.Email.Trim(), _model.Password);
            if (result.Succeeded)
                Navigation.NavigateTo("", forceLoad: true);
            else
                _error = string.Join(" ", result.ErrorList);
        }
        finally
        {
            _busy = false;
        }
    }

    private sealed class LoginModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
