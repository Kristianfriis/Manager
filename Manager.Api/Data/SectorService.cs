using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Manager.Api.Hubs;
using Manager.Api.Models;
using Manager.Shared.Dtos;
using FluentResults;
using Manager.Api.Mappers;

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
                ClaimedCount = s.Zones.Count(z => z.OwnerId != null)
            })
            .ToListAsync();
    }

    public async Task<Result<SectorDetailDto>> GetSectorAsync(int id)
    {
        var entity = await context.Sectors
            .Include(s => s.Zones)
                .ThenInclude(z => z.Owner)
            .FirstOrDefaultAsync(s => s.Id == id);

        return entity is null
            ? Result.Fail("Sector not found")
            : Result.Ok(entity.MapToDetail());
    }

    public async Task<Result<FleetMovementDto>> ClaimZoneAsync(FleetMovement model)
    {
        var zone = await context.Zones.FirstOrDefaultAsync(z => z.Id == model.ZoneId);
        if (zone is null)
            return Result.Fail("Zone not found");
        if (zone.OwnerId is not null)
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
            PlayerId = player.Id,
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
        var dto = movement.MapToMovementDto(zone.Ring, zone.Position);
        await hubContext.Clients.Group($"sector_{zone.SectorId}").SendAsync("MovementUpdated");
        return Result.Ok(dto);
    }

    public async Task<Result<FleetMovementDto>> ReinforceZoneAsync(FleetMovement model)
    {
        var zone = await context.Zones.FirstOrDefaultAsync(z => z.Id == model.ZoneId);
        if (zone is null)
            return Result.Fail("Zone not found");
        if (zone.OwnerId != model.PlayerId)
            return Result.Fail("You do not own this zone");

        var player = await context.Players.FirstOrDefaultAsync(p => p.Id == model.PlayerId);
        if (player is null)
            return Result.Fail("Player not found");

        var fleets = await context.Fleets.Where(f => f.PlayerId == player.Id).ToListAsync();

        var checkResult = CheckFleetComposition(model.FleetComposition, fleets);
        if (checkResult.IsFailed)
            return checkResult;

        var removeResult = RemoveFleetFromPlayer(model.FleetComposition, fleets);
        if (removeResult.IsFailed)
            return removeResult;

        var totalShips = model.FleetComposition.Sum(fc => fc.Count);
        var now = DateTimeOffset.UtcNow;
        var travelMinutes = TravelMinutesByRing[zone.Ring];
        var movement = new FleetMovementEntity
        {
            PlayerId = player.Id,
            PlayerName = player.Name,
            ToZoneId = zone.Id,
            ShipCount = totalShips,
            ShipsMoving = model.FleetComposition.ToDictionary(fc => fc.Type, fc => fc.Count),
            StartTime = now,
            ArrivalTime = now.AddMinutes(travelMinutes),
            IsReinforce = true,
            Resolved = false
        };
        context.FleetMovements.Add(movement);

        await context.SaveChangesAsync();
        var dto = movement.MapToMovementDto(zone.Ring, zone.Position);
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

    public async Task<Result<FleetMovementDto>> AttackZoneAsync(FleetMovement model)
    {
        var zone = await context.Zones.FirstOrDefaultAsync(z => z.Id == model.ZoneId);
        if (zone is null)
            return Result.Fail("Zone not found");

        var player = await context.Players.FirstOrDefaultAsync(p => p.Id == model.PlayerId);
        if (player is null)
            return Result.Fail("Player not found");

        var fleets = await context.Fleets.Where(f => f.PlayerId == player.Id).ToListAsync();

        var checkResult = CheckFleetComposition(model.FleetComposition, fleets);
        if (checkResult.IsFailed)
            return checkResult;

        var removeResult = RemoveFleetFromPlayer(model.FleetComposition, fleets);
        if (removeResult.IsFailed)
            return removeResult;

        var totalShips = model.FleetComposition.Sum(fc => fc.Count);
        var now = DateTimeOffset.UtcNow;
        var travelMinutes = TravelMinutesByRing[zone.Ring];
        var movement = new FleetMovementEntity
        {
            PlayerId = player.Id,
            PlayerName = player.Name,
            ToZoneId = zone.Id,
            ShipCount = totalShips,
            ShipsMoving = model.FleetComposition.ToDictionary(fc => fc.Type, fc => fc.Count),
            StartTime = now,
            ArrivalTime = now.AddMinutes(travelMinutes),
            IsClaim = false,
            Resolved = false
        };
        context.FleetMovements.Add(movement);

        await context.SaveChangesAsync();
        var dto = movement.MapToMovementDto(zone.Ring, zone.Position);
        await hubContext.Clients.Group($"sector_{zone.SectorId}").SendAsync("MovementUpdated");
        return Result.Ok(dto);
    }

    public async Task<List<FleetMovementDto>> GetPlayerMovementsAsync(int? playerId)
    {
        var query = context.FleetMovements.AsQueryable();
        if (playerId.HasValue)
            query = query.Where(m => m.PlayerId == playerId.Value);

        var movements = await query
            .Join(context.Zones.Include(z => z.Owner), m => m.ToZoneId, z => z.Id, (m, z) => new { m, z })
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

        return movements.Select(item => item.m.MapToMovementDto(item.z.Ring, item.z.Position)).ToList();
    }

    public static void ResolveMovement(FleetMovementEntity movement, ZoneEntity zone)
    {
        if (movement.IsReinforce)
        {
            zone.ShipCount += movement.ShipCount;
            foreach (var kvp in movement.ShipsMoving)
            {
                if (zone.ShipsInZone.ContainsKey(kvp.Key))
                    zone.ShipsInZone[kvp.Key] += kvp.Value;
                else
                    zone.ShipsInZone[kvp.Key] = kvp.Value;
            }
            movement.Resolved = true;
            movement.AttackerWon = true;
            movement.RemainingAttackerShips = movement.ShipCount;
            movement.ShipsLost = 0;
            movement.NewOwner = zone.Owner?.Name;
            return;
        }

        if (movement.IsClaim)
        {
            if (zone.OwnerId is not null)
            {
                movement.Resolved = true;
                movement.AttackerWon = false;
                movement.ShipsLost = movement.ShipCount;
                movement.RemainingAttackerShips = 0;
                movement.NewOwner = zone.Owner?.Name;
                return;
            }

            zone.OwnerId = movement.PlayerId;
            zone.ShipCount = movement.ShipCount;
            zone.ShipsInZone = new Dictionary<ShipType, int>(movement.ShipsMoving);
            movement.Resolved = true;
            movement.AttackerWon = true;
            movement.ShipsLost = 0;
            movement.RemainingAttackerShips = movement.ShipCount;
            movement.NewOwner = movement.PlayerName;
            return;
        }

        var defenderFleet = zone.ShipsInZone.Count > 0
            ? new Dictionary<ShipType, int>(zone.ShipsInZone)
            : new Dictionary<ShipType, int> { { ShipType.Fighter, zone.ShipCount } };

        var attackerFleet = new Dictionary<ShipType, int>(movement.ShipsMoving);

        if (defenderFleet.Values.Sum() <= 0)
        {
            zone.OwnerId = movement.PlayerId;
            zone.ShipCount = movement.ShipCount;
            zone.ShipsInZone = attackerFleet;
            movement.Resolved = true;
            movement.AttackerWon = true;
            movement.ShipsLost = 0;
            movement.RemainingAttackerShips = movement.ShipCount;
            movement.NewOwner = movement.PlayerName;
            return;
        }

        var remainingAttacker = new Dictionary<ShipType, int>(attackerFleet);
        var remainingDefender = new Dictionary<ShipType, int>(defenderFleet);
        SimulateCombat(remainingAttacker, remainingDefender);

        var attackerRemaining = remainingAttacker.Values.Sum();
        var defenderRemaining = remainingDefender.Values.Sum();
        var attackerLost = movement.ShipCount - attackerRemaining;

        movement.Resolved = true;
        movement.ShipsLost = attackerLost;
        movement.RemainingAttackerShips = attackerRemaining;

        if (defenderRemaining <= 0 && attackerRemaining > 0)
        {
            zone.OwnerId = movement.PlayerId;
            zone.ShipCount = attackerRemaining;
            zone.ShipsInZone = remainingAttacker;
            movement.AttackerWon = true;
            movement.NewOwner = movement.PlayerName;
        }
        else
        {
            zone.ShipCount = defenderRemaining;
            zone.ShipsInZone = remainingDefender;
            movement.AttackerWon = false;
            movement.NewOwner = zone.Owner?.Name;
        }
    }

    private static void SimulateCombat(Dictionary<ShipType, int> attacker, Dictionary<ShipType, int> defender)
    {
        while (attacker.Values.Sum() > 0 && defender.Values.Sum() > 0)
        {
            var atkDamage = attacker.Sum(kvp => kvp.Value * GameRules.Ships[kvp.Key].AttackDamage);
            var defDamage = defender.Sum(kvp => kvp.Value * GameRules.Ships[kvp.Key].AttackDamage);

            ApplyDamage(attacker, defDamage);
            ApplyDamage(defender, atkDamage);
        }
    }

    private static void ApplyDamage(Dictionary<ShipType, int> ships, int damage)
    {
        var sorted = ships
            .Where(kvp => kvp.Value > 0)
            .OrderBy(kvp => GameRules.Ships[kvp.Key].BaseHealth)
            .ToList();

        foreach (var (type, count) in sorted)
        {
            if (damage <= 0 || !ships.TryGetValue(type, out var current) || current <= 0)
                continue;

            var hp = GameRules.Ships[type].BaseHealth;
            var totalHp = count * hp;

            if (damage >= totalHp)
            {
                damage -= totalHp;
                ships[type] = 0;
            }
            else
            {
                var shipsDestroyed = damage / hp;
                damage -= shipsDestroyed * hp;
                ships[type] -= shipsDestroyed;

                if (damage > 0 && ships[type] > 0)
                {
                    ships[type]--;
                    damage = 0;
                }
            }
        }

        foreach (var key in ships.Where(kvp => kvp.Value <= 0).Select(kvp => kvp.Key).ToList())
            ships.Remove(key);
    }

    public static void Seed(GameDbContext ctx)
    {
        if (ctx.Sectors.Any()) return;

        if (!ctx.Players.Any())
        {
            ctx.Players.Add(new PlayerEntity { Name = "Bot-1" });
            ctx.Players.Add(new PlayerEntity { Name = "Bot-2" });
        }

        ctx.SaveChanges();

        var bots = ctx.Players.Where(p => p.Name.StartsWith("Bot-")).ToDictionary(p => p.Name);

        var sector = new SectorEntity
        {
            Name = "Alpha Sector",
            Zones = GenerateZones(12)
        };
        ctx.Sectors.Add(sector);
        ctx.SaveChanges();

        var rng = new Random(42);
        var botNames = new[] { "Bot-1", "Bot-2" };
        foreach (var zone in sector.Zones.Skip(1).OrderBy(_ => rng.Next()).Take(4))
        {
            var botName = botNames[rng.Next(botNames.Length)];
            zone.OwnerId = bots[botName].Id;
            zone.ShipCount = rng.Next(3, 8);

            var remainingShips = zone.ShipCount;
            foreach (var shipType in Enum.GetValues<ShipType>())
            {
                zone.ShipsInZone[shipType] = rng.Next(0, remainingShips + 1);
                remainingShips -= zone.ShipsInZone[shipType];
            }
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

        return zones;
    }
}
