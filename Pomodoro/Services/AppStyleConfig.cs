namespace Pomodoro.Services;

public class AppStyleConfig
{
    public static string CurrentThemeBackground = Preferences.Get("CurrentThemeBackground", LightTheme);
    public static string CurrentThemeItems = Preferences.Get("CurrentThemeItems", DarkTheme);
    public static string CurrentBootstrapButton = Preferences.Get("CurrentBootstrapButton", DarkBootstrapButton);
    private const string LightBootstrapButton = "light";
    private const string DarkBootstrapButton = "dark";
    private const string DarkTheme = "#404040";
    private const string LightTheme = "white";

    public static void SetDarkTheme()
    {
        CurrentThemeBackground = DarkTheme;
        CurrentThemeItems = LightTheme;
        CurrentBootstrapButton = LightBootstrapButton;
        Preferences.Set("CurrentThemeBackground", CurrentThemeBackground);
        Preferences.Set("CurrentThemeItems", CurrentThemeItems);
        Preferences.Set("CurrentBootstrapButton", CurrentBootstrapButton);
    }

    public static void SetLightTheme()
    {
        CurrentThemeBackground = LightTheme;
        CurrentThemeItems = DarkTheme;
        CurrentBootstrapButton = DarkBootstrapButton;
        Preferences.Set("CurrentThemeBackground", CurrentThemeBackground);
        Preferences.Set("CurrentThemeItems", CurrentThemeItems);
        Preferences.Set("CurrentBootstrapButton", CurrentBootstrapButton);
    }
}
