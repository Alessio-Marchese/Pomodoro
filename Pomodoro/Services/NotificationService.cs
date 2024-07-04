namespace Pomodoro.Services;
using Plugin.LocalNotification;

public class MyNotificationService
{
    public async Task NotifyTimerDone()
    {
        if (await LocalNotificationCenter.Current.AreNotificationsEnabled() == false)
        {
            await LocalNotificationCenter.Current.RequestNotificationPermission();
        }
        var notification = new NotificationRequest
        {
            NotificationId = 100,
            Title = "Timer finito",
            Description = "Torna nell'app per continuare",
            ReturningData = "Dummy data",
        };
        await LocalNotificationCenter.Current.Show(notification);
    }
}
