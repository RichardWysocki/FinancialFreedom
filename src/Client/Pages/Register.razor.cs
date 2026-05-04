using System.Collections.Generic;
using System.Threading.Tasks;
using FinancialFreedom.Client.Identity;
using Microsoft.AspNetCore.Components;

namespace FinancialFreedom.Client.Pages;

public partial class Register
{
    [Inject] private IAccountManagement Account { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private readonly RegisterModel _model = new();
    private readonly List<string> _errors = [];
    private bool _busy;

    private async Task SubmitAsync()
    {
        _errors.Clear();
        if (string.IsNullOrWhiteSpace(_model.Email) || string.IsNullOrWhiteSpace(_model.Password))
        {
            _errors.Add("Enter email and password.");
            return;
        }

        _busy = true;
        try
        {
            var result = await Account.RegisterAsync(_model.Email.Trim(), _model.Password);
            if (result.Succeeded)
            {
                await Account.LoginAsync(_model.Email.Trim(), _model.Password);
                Navigation.NavigateTo("", forceLoad: true);
            }
            else
                _errors.AddRange(result.ErrorList);
        }
        finally
        {
            _busy = false;
        }
    }

    private sealed class RegisterModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
