﻿using Microsoft.Extensions.Logging;
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
            builder.Services.AddSingleton<PomodoroTimer>();
#if ANDROID
            builder.Services.AddTransient<INotificationManagerService, Pomodoro.Platforms.Android.NotificationManagerService>();  
#endif
#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
