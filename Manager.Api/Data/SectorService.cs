using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Manager.Api.Hubs;
using Manager.Api.Models;
using Manager.Shared.Dtos;
using FluentResults;

namespace Manager.Api.Data;

public class SectorService(GameDbContext context, ILogger<SectorService> logger, IHubContext<MovementHub> hubContext)
{
    private static readonly double[] RingBoost = [0.50, 0.30, 0.15, 0.05];
    private static readonly ResourceType[] BoostTypes = [ResourceType.Metal, ResourceType.Energy, ResourceType.Food, ResourceType.Water];
    private static readonly int[] TravelMinutesByRing = [5, 3, 2, 1];

    public async Task<List<SectorDto>> ListSectorsAsync()
    {
        return await context.Sectors
            .Select(s => new SectorDto
            {
                Id = s.Id,
                Name = s.Name,
                ZoneCount = s.Zones.Count,
                ClaimedCount = s.Zones.Count(z => z.OwnerName != null)
            })
            .ToListAsync();
    }

    public async Task<Result<SectorDetailDto>> GetSectorAsync(int id)
    {
        var entity = await context.Sectors
            .Include(s => s.Zones)
            .FirstOrDefaultAsync(s => s.Id == id);

        return entity is null
            ? Result.Fail("Sector not found")
            : Result.Ok(MapToDetail(entity));
    }

    public async Task<Result<FleetMovementDto>> ClaimZoneAsync(FleetMovement model)
    {
        var zone = await context.Zones.FirstOrDefaultAsync(z => z.Id == model.ZoneId);
        if (zone is null)
            return Result.Fail("Zone not found");
        if (zone.OwnerName is not null)
            return Result.Fail("Zone is already claimed");

        var player = await context.Players.FirstOrDefaultAsync(p => p.Id == model.PlayerId);
        if (player is null)
        {
            return Result.Fail("No player found");
        }

        var fleets = await context.Fleets.Where(f => f.PlayerId == player.Id).ToListAsync();

        var totalAvailableShips = fleets.Sum(f => f.Count);

        if (totalAvailableShips < 1)
            return Result.Fail("Not enough ships");

        var checkResult = CheckFleetComposition(model.FleetComposition, fleets);
        if (checkResult.IsFailed)
            return checkResult;

        var SuccessFullyRemovedFleeetFromPlayer = RemoveFleetFromPlayer(model.FleetComposition, fleets);
        if (SuccessFullyRemovedFleeetFromPlayer.IsFailed)
            return SuccessFullyRemovedFleeetFromPlayer;

        var now = DateTimeOffset.UtcNow;
        var travelMinutes = TravelMinutesByRing[zone.Ring];
        var movement = new FleetMovementEntity
        {
            PlayerName = player.Name,
            ToZoneId = zone.Id,
            ShipCount = 1,
            ShipsMoving = model.FleetComposition.ToDictionary(fc => fc.Type, fc => fc.Count),
            StartTime = now,
            ArrivalTime = now.AddMinutes(travelMinutes),
            IsClaim = true,
            Resolved = false
        };
        context.FleetMovements.Add(movement);

        await context.SaveChangesAsync();
        var dto = MapToMovementDto(movement, zone.Ring, zone.Position);
        await hubContext.Clients.Group($"sector_{zone.SectorId}").SendAsync("MovementUpdated");
        return Result.Ok(dto);
    }

    private Result RemoveFleetFromPlayer(
        List<FleetComposition> fleetComposition, 
        List<FleetEntity> fleets)
    {
        foreach (var fc in fleetComposition)
        {
            var matchingFleet = fleets.FirstOrDefault(f => f.Type == fc.Type);
            
            if (matchingFleet is null || matchingFleet.Count < fc.Count)
                return Result.Fail($"Not enough ships of type {fc.Type}");

            matchingFleet.Count -= fc.Count;

            // FIX: If they have no ships left of this type, remove the row from the DB entirely
            if (matchingFleet.Count == 0)
            {
                context.Fleets.Remove(matchingFleet);
            }
        }
        
        return Result.Ok();
    }


    private Result CheckFleetComposition(List<FleetComposition> composition, List<FleetEntity> availableFleets)
    {
        foreach (var fc in composition)
        {
            var matchingFleet = availableFleets.FirstOrDefault(f => f.Type == fc.Type);
            if (matchingFleet is null || matchingFleet.Count < fc.Count)
                return Result.Fail($"Not enough ships of type {fc.Type}");
        }
        return Result.Ok();
    }

    public async Task<Result<FleetMovementDto>> AttackZoneAsync(int zoneId, string attackerName, int shipCount)
    {
        var zone = await context.Zones.FirstOrDefaultAsync(z => z.Id == zoneId);
        if (zone is null)
            return Result.Fail("Zone not found");

        var player = await context.Players.FirstOrDefaultAsync(p => p.Name == attackerName);
        if (player is null)
            return Result.Fail("Player not found");

        var fleets = await context.Fleets.Where(f => f.PlayerId == player.Id).ToListAsync();
        var totalAvailableShips = fleets.Sum(f => f.Count);

        if (totalAvailableShips < shipCount || shipCount <= 0)
            return Result.Fail("Not enough ships");

        var now = DateTimeOffset.UtcNow;
        var travelMinutes = TravelMinutesByRing[zone.Ring];
        var movement = new FleetMovementEntity
        {
            PlayerName = attackerName,
            ToZoneId = zoneId,
            ShipCount = shipCount,
            StartTime = now,
            ArrivalTime = now.AddMinutes(travelMinutes),
            IsClaim = false,
            Resolved = false
        };
        context.FleetMovements.Add(movement);

        await context.SaveChangesAsync();
        var dto = MapToMovementDto(movement, zone.Ring, zone.Position);
        await hubContext.Clients.Group($"sector_{zone.SectorId}").SendAsync("MovementUpdated");
        return Result.Ok(dto);
    }

    public async Task<List<FleetMovementDto>> GetPlayerMovementsAsync(string? playerName)
    {
        var query = context.FleetMovements.AsQueryable();
        if (!string.IsNullOrWhiteSpace(playerName))
            query = query.Where(m => m.PlayerName == playerName);

        var movements = await query
            .Join(context.Zones, m => m.ToZoneId, z => z.Id, (m, z) => new { m, z })
            .ToListAsync();

        var now = DateTimeOffset.UtcNow;
        var resolved = false;
        var affectedSectors = new HashSet<int>();

        foreach (var item in movements)
        {
            if (!item.m.Resolved && now >= item.m.ArrivalTime)
            {
                logger.LogInformation("Resolving movement {MovementId} for player {PlayerName} to zone {ZoneId}", item.m.Id, item.m.PlayerName, item.z.Id);
                ResolveMovement(item.m, item.z);
                resolved = true;
                affectedSectors.Add(item.z.SectorId);
            }
        }

        if (resolved)
        {
            await context.SaveChangesAsync();
            foreach (var sectorId in affectedSectors)
                await hubContext.Clients.Group($"sector_{sectorId}").SendAsync("MovementUpdated");
        }

        return movements.Select(item => MapToMovementDto(item.m, item.z.Ring, item.z.Position)).ToList();
    }

    private void ResolveMovement(FleetMovementEntity movement, ZoneEntity zone)
    {
        if (movement.IsClaim)
        {
            if (zone.OwnerName is not null)
            {
                movement.Resolved = true;
                movement.AttackerWon = false;
                movement.ShipsLost = movement.ShipCount;
                movement.RemainingAttackerShips = 0;
                movement.NewOwner = zone.OwnerName;
                return;
            }

            zone.OwnerName = movement.PlayerName;
            zone.ShipCount = movement.ShipCount;
            movement.Resolved = true;
            movement.AttackerWon = true;
            movement.ShipsLost = 0;
            movement.RemainingAttackerShips = movement.ShipCount;
            movement.NewOwner = movement.PlayerName;
            return;
        }

        var defenderShips = zone.ShipCount;
        var attackerWins = movement.ShipCount > defenderShips;

        movement.Resolved = true;
        movement.ShipsLost = attackerWins ? defenderShips : movement.ShipCount;
        movement.RemainingAttackerShips = attackerWins ? movement.ShipCount - defenderShips : 0;

        if (attackerWins)
        {
            zone.OwnerName = movement.PlayerName;
            zone.ShipCount = movement.ShipCount - defenderShips;
            movement.AttackerWon = true;
            movement.NewOwner = movement.PlayerName;
        }
        else
        {
            zone.ShipCount = defenderShips - movement.ShipCount;
            movement.AttackerWon = false;
            movement.NewOwner = zone.OwnerName;
        }
    }

    public static void Seed(GameDbContext ctx)
    {
        if (ctx.Sectors.Any()) return;

        var sector = new SectorEntity
        {
            Name = "Alpha Sector",
            Zones = GenerateZones(12)
        };
        ctx.Sectors.Add(sector);

        if (!ctx.Players.Any())
        {
            ctx.Players.Add(new PlayerEntity { Name = "Bot-1", });
            ctx.Players.Add(new PlayerEntity { Name = "Bot-2", });
        }

        ctx.SaveChanges();
    }

    private static List<ZoneEntity> GenerateZones(int count)
    {
        var zones = new List<ZoneEntity>();
        var ringSizes = new[] { 1, 3, 4, 4 };
        var pos = 0;
        for (var ring = 0; ring < ringSizes.Length; ring++)
        {
            for (var i = 0; i < ringSizes[ring]; i++)
            {
                zones.Add(new ZoneEntity
                {
                    Ring = ring,
                    Position = pos++,
                    BoostType = BoostTypes[pos % BoostTypes.Length],
                    BoostPercentage = RingBoost[ring],
                    ShipCount = 0
                });
            }
        }

        var bots = new[] { "Bot-1", "Bot-2" };
        var rng = new Random(42);
        foreach (var zone in zones.Skip(1).OrderBy(_ => rng.Next()).Take(4))
        {
            zone.OwnerName = bots[rng.Next(bots.Length)];
            zone.ShipCount = rng.Next(3, 8);
        }

        return zones;
    }

    private static SectorDetailDto MapToDetail(SectorEntity e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Zones = e.Zones.Select(MapZoneToDto).ToList()
    };

    private static ZoneDto MapZoneToDto(ZoneEntity z) => new()
    {
        Id = z.Id,
        SectorId = z.SectorId,
        Ring = z.Ring,
        Position = z.Position,
        BoostType = z.BoostType,
        BoostPercentage = z.BoostPercentage,
        OwnerName = z.OwnerName,
        ShipCount = z.ShipCount
    };

    private static FleetMovementDto MapToMovementDto(FleetMovementEntity m, int ring, int pos) => new()
    {
        Id = m.Id,
        PlayerName = m.PlayerName,
        ToZoneId = m.ToZoneId,
        Ring = ring,
        Position = pos,
        ShipCount = m.ShipCount,
        StartTime = m.StartTime,
        ArrivalTime = m.ArrivalTime,
        IsClaim = m.IsClaim,
        Resolved = m.Resolved,
        AttackerWon = m.AttackerWon,
        RemainingAttackerShips = m.RemainingAttackerShips,
        ShipsLost = m.ShipsLost,
        NewOwner = m.NewOwner,
        FleetComposition = m.ShipsMoving.Select(kvp => new FleetCompositionDto((ShipTypeDto)kvp.Key, kvp.Value)).ToList()
    };
}