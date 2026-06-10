using Manager.Api.Data;
using Manager.Api.Data.Errors;
using Manager.Api.Models;
using Manager.Shared.Dtos;

namespace Manager.Api.Endpoints;

public static class PlanetEndpoints
{
    public static void RegisterPlanetEndpoint(this WebApplication app)
    {
        var group = app.MapGroup("/api/planets").WithTags("Planets");

        group.MapGet("/", async (GameService service) =>
        {
            var planets = await service.ListPlanetsAsync();
            return Results.Ok(planets);
        });

        group.MapGet("/{id:int}", async (int id, GameService service) =>
        {
            var planet = await service.GetPlanetAsync(id);
            return planet is null ? Results.NotFound() : Results.Ok(planet.Value);
        });

        group.MapPost("/", async (CreatePlanetRequest req, GameService service) =>
        {
            var planetResult = await service.CreatePlanetAsync(req.Name, req.PlayerId);
            return planetResult.IsSuccess
                ? Results.Created($"/api/planets/{planetResult.Value.Id}", planetResult.Value)
                : Results.BadRequest();
        });

        group.MapPost("/{id:int}/collect", async (int id, GameService service) =>
        {
            var planetResult = await service.CollectAsync(id);
            return planetResult.IsSuccess
                ? Results.Ok(planetResult.Value)
                : Results.NotFound();
        });

        group.MapPost("/{id:int}/upgrade", async (int id, UpgradeRequest req, GameService service) =>
        {
            var planetResult = await service.UpgradeAsync(id, req.ResourceType);

            if (planetResult.HasError<InsufficientFundsError>())
                return Results.BadRequest("Not enough resources to upgrade");

            return planetResult.IsSuccess
                ? Results.Ok(planetResult.Value)
                : Results.NotFound();
        });

        group.MapPost("/build-ships", async (BuildShipsRequest req, GameService service) =>
        {
            var result = await service.BuildShipsAsync(req.PlayerId, req.Count, (ShipType)req.ShipType, req.PlanetId);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Errors.First().Message);
        });
    }
}