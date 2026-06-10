using Microsoft.AspNetCore.SignalR;

namespace Manager.Api.Hubs;

public class MovementHub : Hub
{
    public async Task JoinSectorGroup(int sectorId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"sector_{sectorId}");
    }

    public async Task LeaveSectorGroup(int sectorId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"sector_{sectorId}");
    }
}
