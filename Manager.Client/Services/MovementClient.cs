using Microsoft.AspNetCore.SignalR.Client;

namespace Manager.Client.Services;

public class MovementClient : IAsyncDisposable
{
    private HubConnection? _hubConnection;

    public event Func<Task>? MovementUpdated;

    public async Task ConnectAsync(string hubUrl, string playerName, int sectorId)
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl($"{hubUrl}/hub/movements")
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On("MovementUpdated", async () =>
        {
            if (MovementUpdated is not null)
                await MovementUpdated.Invoke();
        });

        _hubConnection.Closed += async (error) =>
        {
            await Task.CompletedTask;
        };

        await _hubConnection.StartAsync();
        await _hubConnection.InvokeAsync("JoinSectorGroup", sectorId);
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
}
