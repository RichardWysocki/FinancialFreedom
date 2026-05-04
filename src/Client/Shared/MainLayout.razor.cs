using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;

namespace FinancialFreedom.Client.Shared;

public partial class MainLayout : IDisposable
{
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;

    /// <summary>False until first JS layout pass; then true on desktop, false on narrow screens.</summary>
    private bool _sidebarOpen;

    private bool _viewportReady;

    private string SidebarClass =>
        _sidebarOpen ? "ff-sidebar ff-sidebar--open" : "ff-sidebar";

    private void ToggleSidebar()
    {
        _sidebarOpen = !_sidebarOpen;
    }

    private void CloseSidebar() => _sidebarOpen = false;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        try
        {
            var wide = await JS.InvokeAsync<bool>("ffIsWideLayout");
            _sidebarOpen = wide;
        }
        catch
        {
            _sidebarOpen = true;
        }

        _viewportReady = true;
        Navigation.LocationChanged += OnLocationChanged;
        StateHasChanged();
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        if (!_viewportReady)
            return;

        _ = InvokeAsync(async () =>
        {
            try
            {
                var wide = await JS.InvokeAsync<bool>("ffIsWideLayout");
                if (!wide)
                    _sidebarOpen = false;
            }
            catch
            {
                _sidebarOpen = false;
            }

            StateHasChanged();
        });
    }

    public void Dispose()
    {
        Navigation.LocationChanged -= OnLocationChanged;
    }
}
