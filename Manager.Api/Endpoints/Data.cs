using Manager.Api.Data;
using Manager.Shared.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Manager.Api.Endpoints;

public static class DataEndpoints
{
    public static void RegisterDataEndpoint(this WebApplication app)
    {
        var group = app.MapGroup("/api/data").WithTags("Data");

        group.MapGet("/ship-stats", () =>
        {
            var shipStats = GameRules.Ships;

            var stats = shipStats.Select(s => new ShipStatsDto(
                (ShipTypeDto)s.Key,
                s.Value.Name,
                s.Value.BaseHealth,
                s.Value.AttackDamage,
                s.Value.Speed,
                s.Value.MetalCost
            )).ToList();
        
            return Results.Ok(stats);
        });

    }
}