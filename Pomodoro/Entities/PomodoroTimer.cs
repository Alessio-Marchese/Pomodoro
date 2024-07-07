﻿using System.Timers;
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

    public bool IsActive = false;
    public int AutopilotState;

    //Services
    private static INotificationManagerService NotificationManager;
    public static PomodoroTimer Instance;
    //Constants
    private const int TimerLength = 1000;
    public const int ProductionLength = 25;
    public const int ShortPauseLength = 5;
    public const int LongPauseLength = 10;
    public const int DelayLength = 0;
    private PomodoroTimer(INotificationManagerService notificationManager)
    {
        Instance = this;
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
        if (ElapsedMilliseconds < Time.TotalMilliseconds)
        {
           ElapsedMilliseconds += TimerLength;
           FormattedTime = GetCurrentTime();
            NotificationManager.SendNotification("Timer", this.FormattedTime, this, null);
            NotifyChange.HomeRefresh();
        }
        else
        { 
            NotifyChange.TimerCompleted();
        }
    }
    public void Break()
    {
        IsActive = false;
        NotificationManager.SendNotification("Timer", FormattedTime, this);
        Timer.Stop();
    }
    public void Start()
    {
        IsActive = true;
        NotificationManager.SendNotification("Timer", FormattedTime, this);
        Timer.Start();
    }
    public void SetProduction()
    {
        NotificationManager.DeleteCurrentNotification();
        Name = "Production";
        ElapsedMilliseconds = 0;
        Time = new TimeSpan(0, 0, 0, Preferences.Get("Production", ProductionLength), DelayLength);
        FormattedTime = GetCurrentTime();
        Timer.Stop();
    }
    public void SetShortPause()
    {
        NotificationManager.DeleteCurrentNotification();
        ElapsedMilliseconds = 0;
        Name = "ShortPause";
        Time = new TimeSpan(0, 0, 0, Preferences.Get("ShortPause", ShortPauseLength), DelayLength);
        FormattedTime = GetCurrentTime();
        Timer.Stop();
    }
    public void SetLongPause()
    {
        NotificationManager.DeleteCurrentNotification();
        ElapsedMilliseconds = 0;
        Name = "LongPause";
        Time = new TimeSpan(0, 0, 0, Preferences.Get("LongPause", LongPauseLength), DelayLength);
        FormattedTime = GetCurrentTime();
        Timer.Stop();
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
        FormattedTime = GetCurrentTime();
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
            ResetCurrentTimer();
            NotifyChange.HomeRefresh();
        }
    }
    public static PomodoroTimer GetInstance(INotificationManagerService notificationManagerService)
    {
        if(Instance is null)
        {
            return new PomodoroTimer(notificationManagerService);
        }
        else
        {
            return Instance;
        }
    }

    public void ResetCurrentTimer()
    {
        ElapsedMilliseconds = 0;
        FormattedTime = GetCurrentTime();
        Break();
    }
}
