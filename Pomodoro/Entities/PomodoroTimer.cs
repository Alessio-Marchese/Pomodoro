namespace Pomodoro.Entities;

public class PomodoroTimer
{
    public string Name { get; set; } = string.Empty;
    public int Time { get; set; }

    public PomodoroTimer(string name, int time)
    {
        Name = name;
        Time = time;
    }
}
