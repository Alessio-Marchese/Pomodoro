namespace Pomodoro.Services;

public class NotifyChangeService
{
    public event Action OnChange;
    public void NotifyChange() => OnChange?.Invoke();
}
