using AndroidColor = Android.Graphics.Color;
using Pomodoro.Services;
using Android.Views;

namespace Pomodoro.Platforms.Android;

public class StatusBarService : IStatusBarService
{
    public void RefreshColor()
    {
        var activity = Platform.CurrentActivity;
        if (activity != null)
        {
            var window = activity.Window;
            window.SetStatusBarColor(AndroidColor.ParseColor(AppStyleConfig.CurrentThemeBackground));
            if (AppStyleConfig.IsDarkMode())
            {
                window.DecorView.SystemUiVisibility = 0;
            }
            else
            {
                window.DecorView.SystemUiVisibility = (StatusBarVisibility)SystemUiFlags.LightStatusBar;
            }
        }

        
    }
}
