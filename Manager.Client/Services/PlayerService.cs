using System.Net;
using System.Net.Http.Json;
using Manager.Shared.Dtos;

namespace Manager.Client.Services;

public class PlayerService(IHttpClientFactory httpFactory)
{
    private HttpClient Api => httpFactory.CreateClient("ManagerApi");

    public async Task<(bool success, PlayerDto? player, string? message)> CreatePlayerAsync(string name)
    {
        var response = await Api.PostAsJsonAsync("api/players", new CreatePlayerRequest(name));
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return (false, null, string.IsNullOrWhiteSpace(body) ? "Failed to create player." : body);
        }

        var player = await response.Content.ReadFromJsonAsync<PlayerDto>();
        return (true, player, null);
    }

    public async Task<(bool success, PlayerDto? player, string? message)> GetPlayerAsync(int id)
    {
        var response = await Api.GetAsync($"api/players/{id}");
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
                return (false, null, "Player not found.");

            return (false, null, "Failed to retrieve player.");
        }

        var player = await response.Content.ReadFromJsonAsync<PlayerDto>();
        return (true, player, null);
    }
}
