using Android.Content;
using Pomodoro.Entities;
namespace Pomodoro.Platforms.Android;

[BroadcastReceiver(Enabled = true)]
public class MyBroadcastReceiver : BroadcastReceiver
{
    public override void OnReceive(Context context, Intent intent)
    {
        if (intent.Action == "PAUSE")
        {
            PomodoroTimer.Instance.Break();
        }
        else if(intent.Action == "RESUME")
        {
            PomodoroTimer.Instance.Resume();
        }
        if(intent.Action == "RESET")
        {
            PomodoroTimer.Instance.ResetCurrentTimerFromNotification();
        }
    }
}
