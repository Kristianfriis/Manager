using Manager.Api.Data;
using Manager.Api.Models;
using Manager.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Manager.Api.Endpoints;

public static class FleetEndpoints
{
    public static void RegisterFleetEndpoint(this WebApplication app)
    {
        var group = app.MapGroup("/api/fleet").WithTags("Fleet");

        group.MapGet("/movements", async (string? playerName, SectorService service) =>
        {
            var movements = await service.GetPlayerMovementsAsync(playerName);
            return Results.Ok(movements);
        });

        group.MapGet("/{playerId:int}", async (int playerId, GameDbContext dbContext) =>
        {
            var fleet = await dbContext.Fleets.Where(f => f.PlayerId == playerId).ToListAsync();

            if(fleet == null || fleet.Count == 0)
                return Results.NotFound();

            var mappedtoDto = fleet.Select(f => new FleetDto
            {
                Id = f.Id,
                PlayerId = f.PlayerId,
                Type = (ShipTypeDto)f.Type,
                Count = f.Count,
                Stats = MapToDto(f.Type)
            }).ToList();

            return Results.Ok(mappedtoDto);
        });
    }

    private static ShipStatsDto MapToDto(ShipType type)
    {
        var stats = GameRules.Ships[type];
        return new ShipStatsDto((ShipTypeDto)type, stats.Name, stats.BaseHealth, stats.AttackDamage, stats.Speed, stats.MetalCost);
    }
}