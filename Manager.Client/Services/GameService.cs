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

    public async Task<FleetMovementDto?> ClaimZoneAsync(int zoneId, int playerId, List<FleetCompositionDto> fleet)
    {
        var request = new ClaimRequest(playerId, fleet);
        var response = await Api.PostAsJsonAsync($"api/zones/{zoneId}/claim", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<FleetMovementDto>();
    }

    public async Task<FleetMovementDto?> AttackZoneAsync(int zoneId, int playerId, List<FleetCompositionDto> fleet)
    {
        var request = new ClaimRequest(playerId, fleet);
        var response = await Api.PostAsJsonAsync($"api/zones/{zoneId}/attack", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<FleetMovementDto>();
    }

    public async Task<List<FleetMovementDto>> GetPlayerMovementsAsync(int playerId) =>
        await Api.GetFromJsonAsync<List<FleetMovementDto>>($"api/fleet/movements?playerId={playerId}") ?? [];

    public async Task<ZoneDto?> GetZoneAsync(int zoneId) =>
        await Api.GetFromJsonAsync<ZoneDto>($"api/zones/{zoneId}");

    public async Task<FleetMovementDto?> ReinforceZoneAsync(int zoneId, int playerId, List<FleetCompositionDto> fleet)
    {
        var request = new ClaimRequest(playerId, fleet);
        var response = await Api.PostAsJsonAsync($"api/zones/{zoneId}/reinforce", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<FleetMovementDto>();
    }

    public async Task<PlayerDto?> GetPlayerAsync(int playerId) =>
        await Api.GetFromJsonAsync<PlayerDto>($"api/players/{playerId}");

    public async Task<PlayerDto?> BuildShipsAsync(int playerId, int planetId, ShipTypeDto shipType, int count)
    {
        var request = new BuildShipsRequest(count, shipType, planetId, playerId);
        var response = await Api.PostAsJsonAsync("api/planets/build-ships", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PlayerDto>();
    }

    public async Task<List<FleetDto>> GetFleetAsync(int playerId) =>
        await Api.GetFromJsonAsync<List<FleetDto>>($"api/fleet/{playerId}") ?? [];

    public async Task<List<ShipStatsDto>> GetShipStatsAsync() =>
        await Api.GetFromJsonAsync<List<ShipStatsDto>>("api/data/ship-stats") ?? [];
}
