﻿using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Content;
using Pomodoro.Services;
namespace Pomodoro
{
    [Activity(LaunchMode = LaunchMode.SingleTop, Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            CreateNotificationFromIntent(Intent);
            this.RequestedOrientation = ScreenOrientation.Portrait;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                Window.SetStatusBarColor(Android.Graphics.Color.ParseColor(AppStyleConfig.CurrentThemeBackground));
            }
        }

        protected override void OnNewIntent(Intent? intent)
        {
            base.OnNewIntent(intent);
            CreateNotificationFromIntent(intent);
        }

        static void CreateNotificationFromIntent(Intent intent)
        {
            if (intent?.Extras != null)
            {
                string title = intent.GetStringExtra(Pomodoro.Platforms.Android.NotificationManagerService.TitleKey);
                string message = intent.GetStringExtra(Pomodoro.Platforms.Android.NotificationManagerService.MessageKey);

                var service = IPlatformApplication.Current.Services.GetService<INotificationManagerService>();
                service.ReceiveNotification(title, message);
            }
        }
    }
}
