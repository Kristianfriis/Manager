using System.Net;
using System.Net.Http.Json;
using Manager.Shared.Dtos;

namespace Manager.Client.Services;

public class PlayerService(IHttpClientFactory httpFactory)
{
    private HttpClient Api => httpFactory.CreateClient("ManagerApi");

    public async Task<(bool success, string? message)> CreatePlayerAsync(string name)
    {
        var response = await Api.PostAsJsonAsync("api/players", new { name });
        if (!response.IsSuccessStatusCode)
        {
            return (false, "Failed to create player.");
        }

        return (true, null);
    }

    public async Task<(bool success, string? message)> UpdatePlayerAsync()
    {
        // Implement player update logic if needed
        return (true, null);
    }

    public async Task<(bool success, string? message)> DeletePlayerAsync()
    {
        // Implement player deletion logic if needed
        return (true, null);
    }

    public async Task<(bool success, PlayerDto? player, string? message)> GetPlayerAsync(string name)
    {
        var response = await Api.GetAsync($"api/players/{name}");
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return (false, null, "Player not found.");
            }

            return (false, null, "Failed to retrieve player.");
        }

        var player = await response.Content.ReadFromJsonAsync<PlayerDto>();
        return (true, player, null);
    }
}
