public class NavigationState
{
    public bool IsInSettings { get; private set; }

    // אירוע שמעדכן את התפריט כשמשהו משתנה
    public event Action? OnChange;

    public void SetSettingsMode(bool value)
    {
        IsInSettings = value;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}