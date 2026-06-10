namespace Manager.Client.Services;

public class AppState
{
    public string? PlayerName { get; set; }
    public int PlayerId { get; set; }

    public event Action? OnChange;

    public void SetPlayer(string name, int id)
    {
        PlayerName = name;
        PlayerId = id;
        NotifyStateChanged();
    }

    public void ClearPlayer()
    {
        PlayerName = null;
        PlayerId = 0;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}