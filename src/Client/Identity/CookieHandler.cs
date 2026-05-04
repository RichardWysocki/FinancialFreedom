using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace FinancialFreedom.Client.Identity;

/// <summary>Ensures auth cookies are sent with browser HTTP requests to the host API.</summary>
public sealed class CookieHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        request.Headers.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
        return base.SendAsync(request, cancellationToken);
    }
}
