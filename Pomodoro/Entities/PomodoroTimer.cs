using System;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Pomodoro.Entities;

public class PomodoroTimer
{
    public TimeSpan Time { get; set; }
    public string FormattedTime { get; set; } = string.Empty;
    public Timer Timer { get; set; }

    public event Action OnTimeChanged;
    public PomodoroTimer()
    {
        Timer = new(100);
        Timer.Elapsed += ReduceMilliseconds;
        Timer.AutoReset = true;
        Timer.Enabled = true;
    }

    private void ReduceMilliseconds(Object source, ElapsedEventArgs e)
    {   if(Time.TotalMilliseconds == 0)
        {
            Timer.Stop();
            return;
        }
        Time = Time.Subtract(TimeSpan.FromMilliseconds(100));
        FormattedTime = Time.ToString(@"mm\:ss");
        OnTimeChanged?.Invoke();
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
        Time = new TimeSpan(0, 0, Preferences.Get("Production", 25), 0, 100);
        FormattedTime = Time.ToString(@"mm\:ss");
        Break();
    }

    public void SetShortPause()
    {
        Time = new TimeSpan(0, 0, Preferences.Get("ShortPause", 5), 0, 100);
        FormattedTime = Time.ToString(@"mm\:ss");
        Break();
    }

    public void SetLongPause()
    {
        Time = new TimeSpan(0, 0, Preferences.Get("LongPause", 10), 0, 100);
        FormattedTime = Time.ToString(@"mm\:ss");
        Break();
    }
}
