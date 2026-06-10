using Manager.Api.Data;
using Manager.Api.Mappers;
using Manager.Api.Models;
using Manager.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Manager.Api.Endpoints;

public static class FleetEndpoints
{
    public static void RegisterFleetEndpoint(this WebApplication app)
    {
        var group = app.MapGroup("/api/fleet").WithTags("Fleet");

        group.MapGet("/movements", async (int? playerId, SectorService service) =>
        {
            var movements = await service.GetPlayerMovementsAsync(playerId);
            return Results.Ok(movements);
        });

        group.MapGet("/{playerId:int}", async (int playerId, GameDbContext dbContext) =>
        {
            var fleet = await dbContext.Fleets.Where(f => f.PlayerId == playerId).ToListAsync();

            if (fleet == null || fleet.Count == 0)
                return Results.Ok(new List<FleetDto>());

                var mappedtoDto = fleet.Select(f => f.ToDto()).ToList();

            return Results.Ok(mappedtoDto);
        });
    }
}