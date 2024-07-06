using System.Timers;
using Pomodoro.Services;
using Timer = System.Timers.Timer;


namespace Pomodoro.Entities;

public class PomodoroTimer
{
    public string Name { get; set; } = string.Empty;
    public TimeSpan Time { get; set; }
    public string FormattedTime { get; set; } = string.Empty;
    private Timer Timer { get; set; }
    public bool IsAutopilot { get; set; }
    public int ElapsedMilliseconds { get; set; }

    public int AutopilotState;

    //Services
    private static INotificationManagerService NotificationManager;
    //Constants
    private const int TimerLength = 1000;
    public const int ProductionLength = 25;
    public const int ShortPauseLength = 5;
    public const int LongPauseLength = 10;
    public const int DelayLength = 100;
    public PomodoroTimer(INotificationManagerService notificationManager)
    {
        NotificationManager = notificationManager;
        AutopilotState = Preferences.Get("AutopilotState", 0);
        Timer = new(TimerLength);
        Timer.Elapsed += ReduceMilliseconds;
        Timer.AutoReset = true;
        Timer.Enabled = false;
        if (IsAutopilot)
        {
            SetAutopilot();
        }
        else
        {
            SetProduction();
        }
    }
    private void ReduceMilliseconds(Object source, ElapsedEventArgs e)
    {
        NotificationManager.SendNotification("Timer", "Il timer è iniziato", null, this);
        if (ElapsedMilliseconds < Time.TotalMilliseconds)
        {
            ElapsedMilliseconds += TimerLength;
            FormattedTime = GetCurrentTime();
            NotifyChange.HomeRefresh();
        }
        else
        {
            ElapsedMilliseconds = 0;
            NotifyChange.TimerCompleted();
            return;
        }  
    }
    public void Break()
    {
        Timer.Stop();
    }
    public void Start()
    {
        Timer.Start();
    }
    public void SetProduction()
    {
        NotificationManager.DeleteCurrentNotification();
        Name = "Production";
        ElapsedMilliseconds = 0;
        Time = new TimeSpan(0, 0, 0, Preferences.Get("Production", ProductionLength), DelayLength);
        FormattedTime = Time.ToString(@"mm\:ss");
        Break();
    }
    public void SetShortPause()
    {
        NotificationManager.DeleteCurrentNotification();
        ElapsedMilliseconds = 0;
        Name = "ShortPause";
        Time = new TimeSpan(0, 0, 0, Preferences.Get("ShortPause", ShortPauseLength), DelayLength);
        FormattedTime = Time.ToString(@"mm\:ss");
        Break();
    }
    public void SetLongPause()
    {
        NotificationManager.DeleteCurrentNotification();
        ElapsedMilliseconds = 0;
        Name = "LongPause";
        Time = new TimeSpan(0, 0, 0, Preferences.Get("LongPause", LongPauseLength), DelayLength);
        FormattedTime = Time.ToString(@"mm\:ss");
        Break();
    }
    public void SetAutopilot()
    {
        switch (AutopilotState)
        {
            case 0:
                SetProduction();
                break;
            case 1:
                SetShortPause();
                break;
            case 2:
                SetProduction();
                break;
            case 3:
                SetShortPause();
                break;
            case 4:
                SetProduction();
                break;
            case 5:
                SetLongPause();
                break;
            default:
                ResetAutopilotState();
                break;


        }
        NotifyChange.HomeRefresh();
    }
    public void ResetAutopilotState()
    {
        AutopilotState = 0;
        Preferences.Set("AutopilotState", 0);
        SetAutopilot();
    }
    public void EditTime(TimeSpan time)
    {
        Time = time;
        FormattedTime = time.ToString(@"mm\:ss");
        NotifyChange.HomeRefresh();
    }

    public string GetCurrentTime()
    {
        return Time.Subtract(TimeSpan.FromMilliseconds(ElapsedMilliseconds)).ToString(@"mm\:ss"); 
    }

    public void RestoreTimer()
    {
        if (IsAutopilot)
        {
            if (AutopilotState < 5)
            {
                AutopilotState++;
                SetAutopilot();
            }
            else
            {
                AutopilotState = 0;
            }
            Preferences.Set("AutopilotState", AutopilotState);
        }
        else
        {
            switch (Name.ToUpper())
            {
                case "PRODUCTION":
                    SetProduction();
                    break;
                case "SHORTPAUSE":
                    SetShortPause();
                    break;
                case "LONGPAUSE":
                    SetLongPause();
                    break;
            }
            NotifyChange.HomeRefresh();
        }
    }
}
