using System.Timers;
using Pomodoro.Services;
using Timer = System.Timers.Timer;


namespace Pomodoro.Entities;

public class PomodoroTimer
{   
    public string Name { get; set; } = string.Empty;
    public TimeSpan Time { get; set; }
    public string FormattedTime { get; set; } = string.Empty;
    public Timer Timer { get; set; }

    public event Action OnTimeChanged;

    public event Action OnTimerComplete;
    public bool IsAutopilot { get; set; } = true;
    public int AutopilotState;
    public event Action OnStateChanged;
    public NotifyChangeService NotifyChangeService;
    public PomodoroTimer(NotifyChangeService notifyChangeService)
    {
        NotifyChangeService = notifyChangeService;
        Timer = new(100);
        Timer.Elapsed += ReduceMilliseconds;
        Timer.AutoReset = true;
        Timer.Enabled = false;
        if(IsAutopilot)
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
        if (Time.TotalMilliseconds > 0)
        {
            Time = Time.Subtract(TimeSpan.FromMilliseconds(100));
            FormattedTime = Time.ToString(@"mm\:ss");
            OnTimeChanged?.Invoke();
        }
        if (Time.TotalMilliseconds <= 0)
        {
            if (IsAutopilot)
            {
                SetAutopilot();
                IncreaseAutopilotState();
            }
            else
            {
                Break();
            }
            OnTimerComplete?.Invoke();
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
        Name = "Production";
        Time = new TimeSpan(0, 0, 0, Preferences.Get("Production", 25), 100);
        FormattedTime = Time.ToString(@"mm\:ss");
        Break();
    }

    public void SetShortPause()
    {
        Name = "ShortPause";
        Time = new TimeSpan(0, 0, 0, Preferences.Get("ShortPause", 5), 100);
        FormattedTime = Time.ToString(@"mm\:ss");
        Break();
    }

    public void SetLongPause()
    {
        Name = "LongPause";
        Time = new TimeSpan(0, 0, 0, Preferences.Get("LongPause", 10), 100);
        FormattedTime = Time.ToString(@"mm\:ss");
        Break();
    }

    public void SetAutopilot()
    {
        AutopilotState = Preferences.Get("AutopilotState", 0);
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
        OnStateChanged?.Invoke();
    }
    private void IncreaseAutopilotState()
    {
        if(AutopilotState < 6)
        {
            AutopilotState++;
        }
        else
        {
            AutopilotState = 0;
        }
        Preferences.Set("AutopilotState", AutopilotState);
    }

    public void ResetAutopilotState()
    {
        AutopilotState = 0;
        Preferences.Set("AutopilotState", 0);
        SetAutopilot();
    }

    public void EditTime(TimeSpan time)
    {
        FormattedTime = time.ToString(@"mm\:ss");
        NotifyChangeService.NotifyChange();
    }
}
