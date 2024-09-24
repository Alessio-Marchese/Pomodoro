using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using Pomodoro.Entities;
namespace Pomodoro.Platforms.Android;

public class NotificationManagerService : INotificationManagerService
{
    const string firstChannelId = "Notifications";
    const string firstChannelName = "Notifications";
    const string firstChannelDescription = "The default channel for notifications";

    const string secondChannelId = "Progress";
    const string secondChannelName = "Progress";
    const string secondChannelDescription = "The default channel for progression";

    public const string TitleKey = "title";
    public const string MessageKey = "message";

    bool channelInitialized = false;
    int messageId = 0;
    int pendingIntentId = 0;

    MyRingtoneManager ringtoneManager;

    NotificationManagerCompat compatManager;

    public event EventHandler NotificationReceived;

    public static NotificationManagerService Instance { get; private set; }

    public NotificationManagerService()
    {
        if (Instance == null)
        {
            ringtoneManager = new MyRingtoneManager();
            CreateNotificationChannel();
            compatManager = NotificationManagerCompat.From(Platform.AppContext);
            Instance = this;
        }
    }

    public void SendNotification(string title, string message, DateTime? notifyTime = null)
    {
        if (!channelInitialized)
        {
            CreateNotificationChannel();
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
            Show(title, message);
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

    public void Show(string title, string message)
    {
        PomodoroTimer pomodoroTimer = PomodoroTimer.Instance;
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
            .SetSmallIcon(Resource.Drawable.abc_star_black_48dp);
        if (Build.VERSION.SdkInt < BuildVersionCodes.O)
        {
            builder.SetSound(null);
        }
            int PROGRESS_MAX = (int)pomodoroTimer.Time.TotalMilliseconds;
            int PROGRESS_CURRENT = pomodoroTimer.ElapsedMilliseconds;
            if (PROGRESS_CURRENT < PROGRESS_MAX)
            {
            Intent resetIntent = new Intent(Platform.AppContext, typeof(MyBroadcastReceiver));
            resetIntent.SetAction("RESET");
            PendingIntent resetPendingIntent = PendingIntent.GetBroadcast(Platform.AppContext, 0, resetIntent, pendingIntentFlags);
            builder
                .SetProgress(PROGRESS_MAX, PROGRESS_CURRENT, false)
                .SetOngoing(true);
                if (pomodoroTimer.IsActive)
                {
                    Intent actionIntent = new Intent(Platform.AppContext, typeof(MyBroadcastReceiver));
                    actionIntent.SetAction("PAUSE");
                    PendingIntent actionPendingIntent = PendingIntent.GetBroadcast(Platform.AppContext, 0, actionIntent, pendingIntentFlags);
                    builder.AddAction(Resource.Drawable.m3_radiobutton_ripple, "Pausa", actionPendingIntent);
                }
                else
                {
                    Intent actionIntent = new Intent(Platform.AppContext, typeof(MyBroadcastReceiver));
                    actionIntent.SetAction("RESUME");
                    PendingIntent actionPendingIntent = PendingIntent.GetBroadcast(Platform.AppContext, 0, actionIntent, pendingIntentFlags);
                    builder.AddAction(Resource.Drawable.m3_radiobutton_ripple, pomodoroTimer.ElapsedMilliseconds == 0 ? "Avvia" : "Riprendi", actionPendingIntent);
                }
            builder.AddAction(Resource.Drawable.m3_radiobutton_ripple, "Ripristina", resetPendingIntent);
            compatManager?.Notify(messageId, builder.Build());
            }
            else
            {
                builder
                    .SetChannelId(firstChannelId)
                    .SetOngoing(false)
                    .SetAutoCancel(true)
                    .SetContentText("Timer completato!")
                    .SetProgress(0, 0, false);
                ringtoneManager.PlayCurrentNotificationSound();
                if (Build.VERSION.SdkInt < BuildVersionCodes.O)
                {
                    builder
                    .SetVibrate(new long[] { 0, 250, 250, 250 });
                }
                compatManager.Notify(messageId, builder.Build());
            }
        }

    void CreateNotificationChannel()
    {
        // Create the notification channel, but only on API 26+.
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            NotificationManager manager = (NotificationManager)Platform.AppContext.GetSystemService(Context.NotificationService);
            var firstChannelNameJava = new Java.Lang.String(firstChannelName);
            var firstChannel = new NotificationChannel(firstChannelId, firstChannelNameJava, NotificationImportance.Default)
            {
                Description = firstChannelDescription,
                LockscreenVisibility = NotificationVisibility.Public,
                

            };
            firstChannel.EnableVibration(true);
            var secondChannelNameJava = new Java.Lang.String(secondChannelName);
            var secondChannel = new NotificationChannel(secondChannelId, secondChannelNameJava, NotificationImportance.Default)
            {
                Description = secondChannelDescription,
                LockscreenVisibility = NotificationVisibility.Public
            };
            secondChannel.EnableVibration(false);
            secondChannel.SetSound(null, null);

            // Register the channel
            manager.CreateNotificationChannel(firstChannel);
            manager.CreateNotificationChannel(secondChannel);
            channelInitialized = true;
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