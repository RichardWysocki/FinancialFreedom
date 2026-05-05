using System.Globalization;

namespace FinancialFreedom.Client;

/// <summary>
/// USD formatting for UI. Blazor WASM often runs with invariant culture, where <c>ToString("C")</c>
/// and Mud currency fields without an explicit culture show the generic currency sign (¤) instead of $.
/// </summary>
public static class UiMoney
{
    public static readonly CultureInfo Usd = CultureInfo.GetCultureInfo("en-US");

    public static string Format(decimal value, string format = "C2") => value.ToString(format, Usd);
}
