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
}
