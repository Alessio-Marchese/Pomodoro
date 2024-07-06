using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.OS;
using AndroidX.Core.App;
using Pomodoro.Entities;

namespace Pomodoro.Platforms.Android;

public class NotificationManagerService : INotificationManagerService
{
    const string channelId = "Notifications";
    const string channelName = "Notifications";
    const string channelDescription = "The default channel for notifications";

    const string secondChannelId = "Progress";
    const string secondChannelName = "Progress";
    const string secondChannelDescription = "The default channel for progression";

    public const string TitleKey = "title";
    public const string MessageKey = "message";

    bool channelInitialized = false;
    bool secondChannelInitialized = false;
    int messageId = 0;
    int pendingIntentId = 0;

    NotificationManagerCompat compatManager;

    public event EventHandler NotificationReceived;

    public static NotificationManagerService Instance { get; private set; }

    public NotificationManagerService()
    {
        if (Instance == null)
        {
            CreateNotificationChannel();
            compatManager = NotificationManagerCompat.From(Platform.AppContext);
            Instance = this;
        }
    }

    public void SendNotification(string title, string message, DateTime? notifyTime = null, PomodoroTimer? pomodoroTimer = null)
    {
        if (!channelInitialized)
        {
            CreateNotificationChannel();
        }
        if (!secondChannelInitialized)
        {
            CreateProgressChannel();
        }

        if (notifyTime != null)
        {
            Intent intent = new Intent(Platform.AppContext, typeof(AlarmHandler));
            intent.PutExtra(TitleKey, title);
            intent.PutExtra(MessageKey, message);
            intent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTop);

            var pendingIntentFlags = (Build.VERSION.SdkInt >= BuildVersionCodes.S)
                ? PendingIntentFlags.CancelCurrent | PendingIntentFlags.Immutable
                : PendingIntentFlags.CancelCurrent;

            PendingIntent pendingIntent = PendingIntent.GetBroadcast(Platform.AppContext, pendingIntentId++, intent, pendingIntentFlags);
            long triggerTime = GetNotifyTime(notifyTime.Value);
            AlarmManager alarmManager = Platform.AppContext.GetSystemService(Context.AlarmService) as AlarmManager;
            alarmManager.Set(AlarmType.RtcWakeup, triggerTime, pendingIntent);
        }
        else
        {
            if(pomodoroTimer != null)
            {
                Show(title, message, pomodoroTimer);
            }
            else
            {
                Show(title, message);
            }
        }
    }

    public void ReceiveNotification(string title, string message)
    {
        var args = new NotificationEventArgs()
        {
            Title = title,
            Message = message,
        };
        NotificationReceived?.Invoke(null, args);
    }

    public void Show(string title, string message, PomodoroTimer? pomodoroTimer = null)
    {
        Intent intent = new Intent(Platform.AppContext, typeof(MainActivity));
        intent.PutExtra(TitleKey, title);
        intent.PutExtra(MessageKey, message);
        intent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTop);

        var pendingIntentFlags = (Build.VERSION.SdkInt >= BuildVersionCodes.S)
            ? PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable
            : PendingIntentFlags.UpdateCurrent;

        PendingIntent pendingIntent = PendingIntent.GetActivity(Platform.AppContext, pendingIntentId++, intent, pendingIntentFlags);
        NotificationCompat.Builder builder = new NotificationCompat.Builder(Platform.AppContext)
            .SetChannelId(secondChannelId)
            .SetContentIntent(pendingIntent)
            .SetContentTitle(title)
            .SetContentText(message)
            .SetLargeIcon(BitmapFactory.DecodeResource(Platform.AppContext.Resources, Resource.Drawable.dotnet_bot))
            .SetSmallIcon(Resource.Drawable.notification_action_background);
        if (Build.VERSION.SdkInt < BuildVersionCodes.O)
        {
            builder.SetSound(null);
        }
        if (pomodoroTimer != null)
        {
            int PROGRESS_MAX = (int)pomodoroTimer.Time.TotalMilliseconds;
                if(pomodoroTimer.ElapsedMilliseconds < PROGRESS_MAX)
                {

                builder
                .SetProgress(PROGRESS_MAX, (int)pomodoroTimer.ElapsedMilliseconds, false)
                .SetOngoing(true);
                    if(pomodoroTimer.IsActive)
                    {
                    Intent actionIntent = new Intent(Platform.AppContext, typeof(MyBroadcastReceiver));
                    actionIntent.SetAction("PAUSE");
                    PendingIntent actionPendingIntent = PendingIntent.GetBroadcast(Platform.AppContext, 0, actionIntent, pendingIntentFlags);
                    builder.AddAction(Resource.Drawable.m3_radiobutton_ripple, "Pause", actionPendingIntent);
                    }
                    else
                    {
                    Intent actionIntent = new Intent(Platform.AppContext, typeof(MyBroadcastReceiver));
                    actionIntent.SetAction("RESUME");
                    PendingIntent actionPendingIntent = PendingIntent.GetBroadcast(Platform.AppContext, 0, actionIntent, pendingIntentFlags);
                    builder.AddAction(Resource.Drawable.m3_radiobutton_ripple, "Resume", actionPendingIntent);
                }
                compatManager.Notify(messageId, builder.Build());
                }
                else
                {
                builder
                    .SetChannelId(channelId)
                    .SetOngoing(false)
                    .SetAutoCancel(true)
                    .SetContentText("Timer completato!")
                    .SetProgress(0, 0, false);
                Ringtone? r = RingtoneManager.GetRingtone(Platform.AppContext, RingtoneManager.GetDefaultUri(RingtoneType.Notification));
                r?.Play();
                if (Build.VERSION.SdkInt < BuildVersionCodes.O)
                {
                    builder
                    .SetVibrate(new long[] { 0, 250, 250, 250 });
                }
                compatManager.Notify(messageId, builder.Build());
                pomodoroTimer.Break();
                }
        }
        else
        {
            builder
                .SetChannelId(channelId)
                .SetAutoCancel(true);
            Ringtone? r = RingtoneManager.GetRingtone(Platform.AppContext, RingtoneManager.GetDefaultUri(RingtoneType.Notification));
            r?.Play();
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                builder
                .SetVibrate(new long[] { 0, 250, 250, 250 });
            }
            compatManager.Notify(messageId++, builder.Build());
        }
    }

    void CreateNotificationChannel()
    {
        // Create the notification channel, but only on API 26+.
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var channelNameJava = new Java.Lang.String(channelName);
            var channel = new NotificationChannel(channelId, channelNameJava, NotificationImportance.Default)
            {
                Description = channelDescription
            };
            channel.EnableVibration(true);
            // Register the channel
            NotificationManager manager = (NotificationManager)Platform.AppContext.GetSystemService(Context.NotificationService);
            manager.CreateNotificationChannel(channel);
            channelInitialized = true;
        }
    }

    void CreateProgressChannel()
    {
        // Create the notification channel, but only on API 26+.
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var channelNameJava = new Java.Lang.String(secondChannelName);
            var channel = new NotificationChannel(secondChannelId, channelNameJava, NotificationImportance.Default)
            {
                Description = secondChannelDescription,

            };
            channel.SetSound(null, null);
            // Register the channel
            NotificationManager manager = (NotificationManager)Platform.AppContext.GetSystemService(Context.NotificationService);
            manager.CreateNotificationChannel(channel);
            secondChannelInitialized = true;
        }
    }

    long GetNotifyTime(DateTime notifyTime)
    {
        DateTime utcTime = TimeZoneInfo.ConvertTimeToUtc(notifyTime);
        double epochDiff = (new DateTime(1970, 1, 1) - DateTime.MinValue).TotalSeconds;
        long utcAlarmTime = utcTime.AddSeconds(-epochDiff).Ticks / 10000;
        return utcAlarmTime; // milliseconds
    }

    public void DeleteCurrentNotification()
    {
        NotificationManager manager = (NotificationManager)Platform.AppContext.GetSystemService(Context.NotificationService);
        manager.Cancel(messageId);
    }
}