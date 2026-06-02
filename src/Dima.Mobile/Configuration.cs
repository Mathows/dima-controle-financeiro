using MudBlazor;
using MudBlazor.Utilities;

namespace Dima.Mobile;

public static class Configuration
{
    public const string HttpClientName = "dima-mobile";

    public static string BackendUrl { get; set; } = "https://dima-api-matheus.azurewebsites.net";

    public static MudTheme Theme = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = new MudColor("#1EFA2D"),
            PrimaryContrastText = new MudColor("#000000"),
            AppbarBackground = new MudColor("#1EFA2D"),
            AppbarText = new MudColor("#000000")
        },
        PaletteDark = new PaletteDark
        {
            Primary = new MudColor("#76FF03"),
            PrimaryContrastText = new MudColor("#000000"),
            AppbarBackground = new MudColor("#76FF03"),
            AppbarText = new MudColor("#000000")
        }
    };
}
