using System.Net.Http.Json;
using Manager.Shared.Dtos;

namespace Manager.Client.Services;

public class GameService(IHttpClientFactory httpFactory)
{
    private HttpClient Api => httpFactory.CreateClient("ManagerApi");

    public async Task<IEnumerable<PlanetLookupDto>> ListPlanetsAsync() =>
        await Api.GetFromJsonAsync<IEnumerable<PlanetLookupDto>>("api/planets") ?? [];

    public async Task<PlanetDto?> GetPlanetAsync(int id) =>
        await Api.GetFromJsonAsync<PlanetDto>($"api/planets/{id}");

    public async Task<PlanetDto?> CreatePlanetAsync(string name)
    {
        var response = await Api.PostAsJsonAsync("api/planets", new { name });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PlanetDto>();
    }

    public async Task<PlanetDto?> CollectAsync(int id)
    {
        var response = await Api.PostAsync($"api/planets/{id}/collect", null);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PlanetDto>();
    }

    public async Task<PlanetDto?> UpgradeAsync(int id, ResourceType type)
    {
        var response = await Api.PostAsJsonAsync($"api/planets/{id}/upgrade", new { resourceType = type });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PlanetDto>();
    }

    public async Task<List<SectorDto>> ListSectorsAsync() =>
        await Api.GetFromJsonAsync<List<SectorDto>>("api/sectors") ?? [];

    public async Task<SectorDetailDto?> GetSectorAsync(int id) =>
        await Api.GetFromJsonAsync<SectorDetailDto>($"api/sectors/{id}");

    public async Task<FleetMovementDto?> ClaimZoneAsync(int zoneId, string playerName)
    {
        var response = await Api.PostAsJsonAsync($"api/zones/{zoneId}/claim", new { playerName });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<FleetMovementDto>();
    }

    public async Task<FleetMovementDto?> AttackZoneAsync(int zoneId, string playerName, int shipCount)
    {
        var response = await Api.PostAsJsonAsync($"api/zones/{zoneId}/attack", new { playerName, shipCount });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<FleetMovementDto>();
    }

    public async Task<List<FleetMovementDto>> GetPlayerMovementsAsync(string playerName) =>
        await Api.GetFromJsonAsync<List<FleetMovementDto>>($"api/fleet/movements?playerName={playerName}") ?? [];

    public async Task<PlayerDto?> GetPlayerAsync(string name) =>
        await Api.GetFromJsonAsync<PlayerDto>($"api/players/{name}");

    public async Task<PlayerDto?> BuildShipsAsync(string playerName, int count)
    {   
        var request = new BuildShipsRequest(count, ShipTypeDto.Fighter, 1, 1); // TODO: pass actual ship type, planet id and player id
        var response = await Api.PostAsJsonAsync($"api/planets/build-ships", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PlayerDto>();
    }
}
