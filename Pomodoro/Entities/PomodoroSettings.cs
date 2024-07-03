namespace Pomodoro.Entities;

public class PomodoroSettings
{
    public string Name { get; set; } = string.Empty;
    public int Time { get; set; }

    public PomodoroSettings(string name, int time)
    {
        Name = name;
        Time = time;
    }
}
