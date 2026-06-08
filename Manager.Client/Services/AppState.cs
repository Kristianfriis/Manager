namespace Manager.Client.Services;

public class AppState
{
    public string? PlayerName { get; set; }

    public event Action? OnChange;

    public void SetPlayerName(string name)
    {
        PlayerName = name;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}