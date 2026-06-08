using Microsoft.EntityFrameworkCore;
using Manager.Api.Models;
using Manager.Shared.Dtos;
using FluentResults;

namespace Manager.Api.Data;

public class SectorService(GameDbContext context)
{
    private static readonly double[] RingBoost = [0.50, 0.30, 0.15, 0.05];
    private static readonly ResourceType[] BoostTypes = [ResourceType.Metal, ResourceType.Energy, ResourceType.Food, ResourceType.Water];

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

    public async Task<Result<ZoneDto>> ClaimZoneAsync(int zoneId, string playerName)
    {
        var zone = await context.Zones.FirstOrDefaultAsync(z => z.Id == zoneId);
        if (zone is null)
            return Result.Fail("Zone not found");
        if (zone.OwnerName is not null)
            return Result.Fail("Zone is already claimed");

        zone.OwnerName = playerName;
        zone.ShipCount = 1;

        var player = await context.Players.FirstOrDefaultAsync(p => p.Name == playerName);
        if (player is null)
        {
            player = new PlayerEntity { Name = playerName, ShipCount = 10, Metal = 200 };
            context.Players.Add(player);
        }

        if (player.ShipCount < 1)
            return Result.Fail("Not enough ships");

        player.ShipCount -= 1;
        await context.SaveChangesAsync();
        return Result.Ok(MapZoneToDto(zone));
    }

    public async Task<Result<AttackResultDto>> AttackZoneAsync(int zoneId, string attackerName, int shipCount)
    {
        var zone = await context.Zones.FirstOrDefaultAsync(z => z.Id == zoneId);
        if (zone is null)
            return Result.Fail("Zone not found");

        var player = await context.Players.FirstOrDefaultAsync(p => p.Name == attackerName);
        if (player is null || player.ShipCount < shipCount || shipCount <= 0)
            return Result.Fail("Not enough ships");

        player.ShipCount -= shipCount;

        var defenderShips = zone.ShipCount;
        var attackerWins = shipCount > defenderShips;

        if (attackerWins)
        {
            zone.OwnerName = attackerName;
            zone.ShipCount = shipCount - defenderShips;

            if (player.ShipCount < 0)
                player.ShipCount = 0;

            await context.SaveChangesAsync();
            return Result.Ok(new AttackResultDto
            {
                AttackerWon = true,
                RemainingAttackerShips = zone.ShipCount,
                ShipsLost = defenderShips,
                NewOwner = attackerName
            });
        }

        zone.ShipCount = defenderShips - shipCount;
        player.ShipCount += 0;

        await context.SaveChangesAsync();
        return Result.Ok(new AttackResultDto
        {
            AttackerWon = false,
            RemainingAttackerShips = 0,
            ShipsLost = shipCount,
            NewOwner = zone.OwnerName
        });
    }

    public async Task<Result<PlayerDto>> BuildShipsAsync(string playerName, int count)
    {
        if (count <= 0)
            return Result.Fail("Must build at least 1 ship");

        var player = await context.Players.FirstOrDefaultAsync(p => p.Name == playerName);
        if (player is null)
            return Result.Fail("Player not found");

        const int costPerShip = 10;
        var totalCost = count * costPerShip;

        if (player.Metal < totalCost)
            return Result.Fail($"Not enough Metal. Need {totalCost}, have {player.Metal}.");

        player.Metal -= totalCost;
        player.ShipCount += count;
        await context.SaveChangesAsync();

        return Result.Ok(new PlayerDto { Id = player.Id, Name = player.Name, ShipCount = player.ShipCount, Metal = player.Metal });
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
            ctx.Players.Add(new PlayerEntity { Name = "Bot-1", ShipCount = 25, Metal = 500 });
            ctx.Players.Add(new PlayerEntity { Name = "Bot-2", ShipCount = 25, Metal = 500 });
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
}


