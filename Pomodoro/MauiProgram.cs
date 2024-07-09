using Microsoft.Extensions.Logging;
using Pomodoro.Entities;
using Pomodoro.Services;

namespace Pomodoro
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });
           
            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddSingleton<PomodoroTimer>(sp =>
            {
                return PomodoroTimer.GetInstance(sp.GetRequiredService<INotificationManagerService>());
            });
#if ANDROID
                builder.Services.AddSingleton<INotificationManagerService, Pomodoro.Platforms.Android.NotificationManagerService>();
#endif

            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();

            
            return builder.Build();
        }
    }
}
