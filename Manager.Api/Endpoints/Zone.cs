using Manager.Api.Data;
using Manager.Api.Models;
using Manager.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Manager.Api.Endpoints;

public static class ZoneEndpoints
{
    public static void RegisterZoneEndpoint(this WebApplication app)
    {
        var group = app.MapGroup("/api/zones").WithTags("Zones");

        group.MapPost("/{id:int}/claim", async (int id, ClaimRequest req, SectorService service) =>
        {
            var model = new FleetMovement()
            {
                PlayerId = req.PlayerId,
                ZoneId = id,
                FleetComposition = req.Fleet.Select(f => new FleetComposition((ShipType)f.Type, f.Count)).ToList()
            };
            var result = await service.ClaimZoneAsync(model);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Errors.First().Message);
        });

        group.MapPost("/{id:int}/attack", async (int id, ClaimRequest req, SectorService service) =>
        {
            var model = new FleetMovement()
            {
                PlayerId = req.PlayerId,
                ZoneId = id,
                FleetComposition = req.Fleet.Select(f => new FleetComposition((ShipType)f.Type, f.Count)).ToList()
            };
            var result = await service.AttackZoneAsync(model);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Errors.First().Message);
        });

        group.MapPost("/{id:int}/reinforce", async (int id, ClaimRequest req, SectorService service) =>
        {
            var model = new FleetMovement()
            {
                PlayerId = req.PlayerId,
                ZoneId = id,
                FleetComposition = req.Fleet.Select(f => new FleetComposition((ShipType)f.Type, f.Count)).ToList()
            };
            var result = await service.ReinforceZoneAsync(model);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Errors.First().Message);
        });

        group.MapGet("/{id:int}", async (int id, GameDbContext db) =>
        {
            var zone = await db.Zones.Include(z => z.Owner).FirstOrDefaultAsync(z => z.Id == id);
            if (zone == null)
                return Results.NotFound();

            var mapped = new ZoneDto()
            {
                Id = zone.Id,
                SectorId = zone.SectorId,
                Ring = zone.Ring,
                Position = zone.Position,
                BoostType = zone.BoostType,
                BoostPercentage = zone.BoostPercentage,
                OwnerId = zone.OwnerId,
                OwnerName = zone.Owner?.Name,
                ShipCount = zone.ShipCount,
                FleetComposition = zone.ShipsInZone.Select(kvp => new FleetCompositionDto((ShipTypeDto)kvp.Key, kvp.Value)).ToArray()
            };

            return Results.Ok(mapped);
        });

    }
}
