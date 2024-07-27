namespace Pomodoro.Services;

public class NotifyChange
{
    public static event Action OnChange;
    public static event Action OnTimerComplete;
    public static event Action NavBarChange;
    public static void TimerCompleted() => OnTimerComplete?.Invoke();
    public static void HomeRefresh() => OnChange?.Invoke();
    public static void NavBarRefresh() => NavBarChange?.Invoke();

    
}
