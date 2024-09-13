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
    public int ElapsedMilliseconds { get; set; } = 10000;
    public bool IsDefaultSound { get; set; }

    public bool IsActive = false;
    public int AutopilotState;
    public double StrokeDashOffSet = Math.Round(2 * Math.PI * CircleRadius);
    public int CurrentStrokeDashOffet;

    //Services
    private static INotificationManagerService NotificationManager;
    public static PomodoroTimer Instance;
    //Constants
    private const int TimerLength = 1000;
    public const int ProductionLength = 25;
    public const int ShortPauseLength = 5;
    public const int LongPauseLength = 10;
    public const int DelayLength = 0;
    public const int CircleRadius = 46;
    
    private PomodoroTimer(INotificationManagerService notificationManager)
    {
        if(Instance == null)
        {
            IsDefaultSound = Preferences.Get("IsDefaultSound", 1) == 0 ? false : true;
            NotificationManager = notificationManager;
            AutopilotState = Preferences.Get("AutopilotState", 0);
            CalculateStrokeDashOffset();
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
            Instance = this;
        }
    }
    private void ReduceMilliseconds(Object source, ElapsedEventArgs e)
    {
        if (ElapsedMilliseconds < Time.TotalMilliseconds)
        {
           ElapsedMilliseconds += TimerLength;
           FormattedTime = GetCurrentTime();
            CalculateStrokeDashOffset();
            NotificationManager.SendNotification("Timer", this.FormattedTime);
            NotifyChange.HomeRefresh();
        }
        else
        {
            //NotifyChange.TimerCompleted();
            RestoreTimer();
        }
    }
    public void Break()
    {
        IsActive = false;
        NotifyChange.HomeRefresh();
        NotificationManager.SendNotification("Timer", FormattedTime);
        Timer.Stop();
    }
    public void Start()
    {
        NotificationManager.DeleteCurrentNotification();
        IsActive = true;
        NotifyChange.HomeRefresh();
        NotificationManager.SendNotification("Timer", FormattedTime);
        Timer.Start();
    }
    public void Resume()
    {
        IsActive = true;
        NotifyChange.HomeRefresh();
        NotificationManager.SendNotification("Timer", FormattedTime);
        Timer.Start();
    }
    public void SetProduction()
    {
        Name = "Production";
        ElapsedMilliseconds = 0;
        CalculateStrokeDashOffset();
        Time = new TimeSpan(0, 0, 0, Preferences.Get("Production", ProductionLength), DelayLength);
        FormattedTime = GetCurrentTime();
        Timer.Stop();
        IsActive = false;
    }
    public void SetShortPause()
    {
        Name = "ShortPause";
        ElapsedMilliseconds = 0;
        CalculateStrokeDashOffset();
        Time = new TimeSpan(0, 0, 0, Preferences.Get("ShortPause", ShortPauseLength), DelayLength);
        FormattedTime = GetCurrentTime();
        Timer.Stop();
        IsActive = false;
    }
    public void SetLongPause()
    {
        Name = "LongPause";
        ElapsedMilliseconds = 0;
        CalculateStrokeDashOffset();
        Time = new TimeSpan(0, 0, 0, Preferences.Get("LongPause", LongPauseLength), DelayLength);
        FormattedTime = GetCurrentTime();
        Timer.Stop();
        IsActive = false;
    }
    public void UpdateProduction()
    {
        CalculateStrokeDashOffset();
        Time = new TimeSpan(0, 0, 0, Preferences.Get("Production", ProductionLength), DelayLength);
        FormattedTime = GetCurrentTime();
        if(IsActive)
        {
            NotificationManager.SendNotification("Timer", this.FormattedTime);
        }
    }
    public void UpdateShortPause()
    {
        CalculateStrokeDashOffset();
        Time = new TimeSpan(0, 0, 0, Preferences.Get("ShortPause", ShortPauseLength), DelayLength);
        FormattedTime = GetCurrentTime();
        if (IsActive)
        {
            NotificationManager.SendNotification("Timer", this.FormattedTime);
        }
    }
    public void UpdateLongPause()
    {
        CalculateStrokeDashOffset();
        Time = new TimeSpan(0, 0, 0, Preferences.Get("LongPause", LongPauseLength), DelayLength); ;
        FormattedTime = GetCurrentTime();
            if (IsActive)
            {
                NotificationManager.SendNotification("Timer", this.FormattedTime);
            }
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


        }
        NotifyChange.HomeRefresh();
    }
    public void ResetAutopilotState()
    {
        AutopilotState = 0;
        Preferences.Set("AutopilotState", 0);
        SetAutopilot();
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
            }
            else
            {
                AutopilotState = 0;
            }
            SetAutopilot();
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

    public void ResetCurrentTimer()
    {
        IsActive = false;
        ElapsedMilliseconds = 0;
        FormattedTime = GetCurrentTime();
        CalculateStrokeDashOffset();
        Timer.Stop();
        NotifyChange.HomeRefresh();
    }

    public void ResetCurrentTimerFromNotification()
    {
        IsActive = false;
        ElapsedMilliseconds = 0;
        FormattedTime = GetCurrentTime();
        CalculateStrokeDashOffset();
        Timer.Stop();
        NotifyChange.HomeRefresh();
        NotificationManager.SendNotification("Timer", FormattedTime);
    }



    public static PomodoroTimer GetInstance(INotificationManagerService notificationManagerService)
    {
        if(Instance is null)
        {
            Instance = new PomodoroTimer(notificationManagerService);
        }
        return Instance;
    }

    public void CalculateStrokeDashOffset()
    {
        double percentageElapsed = ElapsedMilliseconds / Time.TotalMilliseconds;
        CurrentStrokeDashOffet = (int)(StrokeDashOffSet * percentageElapsed);
    }
}
