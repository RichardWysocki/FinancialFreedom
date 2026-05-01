using FinancialFreedom.Client.Identity.Models;

namespace FinancialFreedom.Client.Identity;

public interface IAccountManagement
{
    Task<FormResult> RegisterAsync(string email, string password);
    Task<FormResult> LoginAsync(string email, string password);
    Task LogoutAsync();
    Task<bool> CheckAuthenticatedAsync();
}
