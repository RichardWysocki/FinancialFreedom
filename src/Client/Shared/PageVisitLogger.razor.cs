using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FinancialFreedom.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace FinancialFreedom.Client.Shared;

public partial class PageVisitLogger : IDisposable
{
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private HttpClient Http { get; set; } = default!;

    private bool _hooked;
    private string? _lastPathAndQuery;

    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender)
            return;

        Navigation.LocationChanged += OnLocationChanged;
        _hooked = true;
        _ = LogAsync(Navigation.Uri);
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        _ = LogAsync(Navigation.ToAbsoluteUri(e.Location).ToString());
    }

    private async Task LogAsync(string uri)
    {
        try
        {
            Uri absolute;
            if (Uri.TryCreate(uri, UriKind.Absolute, out var abs))
                absolute = abs;
            else
                absolute = Navigation.ToAbsoluteUri(uri);
            var pathAndQuery = absolute.PathAndQuery;
            if (string.Equals(pathAndQuery, _lastPathAndQuery, StringComparison.Ordinal))
                return;

            _lastPathAndQuery = pathAndQuery;

            var qIndex = pathAndQuery.IndexOf('?', StringComparison.Ordinal);
            var path = qIndex >= 0 ? pathAndQuery[..qIndex] : pathAndQuery;
            var query = qIndex >= 0 && qIndex < pathAndQuery.Length - 1
                ? pathAndQuery[(qIndex + 1)..]
                : null;

            await Http.PostAsJsonAsync("api/PageVisitLog", new PageVisitLogRequest
            {
                PagePath = path,
                QueryString = query,
            });
        }
        catch
        {
            // Do not affect navigation if logging fails.
        }
    }

    public void Dispose()
    {
        if (_hooked)
            Navigation.LocationChanged -= OnLocationChanged;
    }
}
