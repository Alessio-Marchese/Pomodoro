namespace Pomodoro.Services;

public class RefreshHomePage
{
    public event Action OnChange;
    public void Refresh() => OnChange?.Invoke();
}
