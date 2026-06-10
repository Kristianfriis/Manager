using Manager.Api.Data;
using Manager.Api.Models;
using Manager.Shared.Dtos;

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
                Id = id,
                PlayerId = req.PlayerId,
                ZoneId = id,
                FleetComposition = req.Fleet.Select(f => new FleetComposition((ShipType)f.Type, f.Count)).ToList(),
                ArrivalTime = DateTimeOffset.UtcNow.AddMinutes(5) // Example travel time
            };
            var result = await service.ClaimZoneAsync(model);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Errors.First().Message);
        });

        group.MapPost("/{id:int}/attack", async (int id, AttackRequest req, SectorService service) =>
        {
            var result = await service.AttackZoneAsync(id, req.PlayerName, req.ShipCount);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Errors.First().Message);
        });

    }
}