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
    private const string MoonColor = "#011044";
    private const string SunColor = "#da6700";
    private const string DefaultMoonSunColor = "grey";

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

    public static string GetMoonColor()
    {
        if(CurrentThemeBackground.Equals(DarkTheme))
        {
            return MoonColor;
        }
        return DefaultMoonSunColor;
    }

    public static string GetSunColor()
    {
        if (CurrentThemeBackground.Equals(LightTheme))
        {
            return SunColor;
        }
        return DefaultMoonSunColor;
    }

    public static bool IsDarkMode()
    {
        if(CurrentThemeBackground.Equals(DarkTheme))
        {
            return true;
        }
        return false;
    }
}
