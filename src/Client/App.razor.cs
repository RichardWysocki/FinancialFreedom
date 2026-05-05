using MudBlazor;

namespace FinancialFreedom.Client;

public partial class App
{
    /// <summary>
    /// MudSelect/MudMenu/Numeric popovers default below or equal to dialogs, so dropdowns render
    /// under the overlay and clicks never reach items. Bump popover strictly above dialogs.
    /// </summary>
    private static readonly MudTheme FfMudTheme = new()
    {
        ZIndex = new ZIndex
        {
            AppBar = 1100,
            Drawer = 1200,
            Dialog = 1300,
            Popover = 1450,
            Snackbar = 2000,
            Tooltip = 2000,
        },
    };
}
