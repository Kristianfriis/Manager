using Microsoft.EntityFrameworkCore;
using Manager.Api.Models;
using Manager.Shared.Dtos;
using FluentResults;
using Manager.Api.Data.Errors;

namespace Manager.Api.Data;

public class GameService(GameDbContext context)
{
    public async Task<Result<PlanetDto>> GetPlanetAsync(int id)
    {
        var entity = await QueryPlanet().FirstOrDefaultAsync(p => p.Id == id);
        return entity == null ? Result.Fail("Planet not found") : await RecalculateAndSaveAsync(entity);
    }

    public async Task<Result<PlanetDto>> CreatePlanetAsync(string name)
    {
        var planet = new PlanetEntity { Name = name, Population = 10 };
        planet.New();

        context.Planets.Add(planet);
        await context.SaveChangesAsync();
        return Result.Ok(MapToDto(planet));
    }

    public async Task<Result<PlanetDto>> CollectAsync(int id)
    {
        var entity = await QueryPlanet().FirstOrDefaultAsync(p => p.Id == id);
        return entity == null ? Result.Fail("Planet not found") : await RecalculateAndSaveAsync(entity);
    }

    public async Task<Result<PlanetDto>> UpgradeAsync(int id, ResourceType type)
    {
        var entity = await QueryPlanet().FirstOrDefaultAsync(p => p.Id == id);
        if (entity == null) return Result.Fail("Planet not found");

        ResourceCalculator.Recalculate(entity);
        
        var prod = entity.Productions.FirstOrDefault(p => p.Type == type);
        if (prod == null) return Result.Fail("Production not found");

        var metal = entity.Resources.FirstOrDefault(r => r.Type == ResourceType.Metal);
        var energy = entity.Resources.FirstOrDefault(r => r.Type == ResourceType.Energy);

        if (metal == null || energy == null) 
            return new InsufficientFundsError();
        if (metal.Amount < prod.MetalToUpgrade || energy.Amount < prod.EnergyNeededForUpgrade) 
            return new InsufficientFundsError();

        metal.Amount -= (int)prod.MetalToUpgrade;
        energy.Amount -= (int)prod.EnergyNeededForUpgrade;
        prod.Level++;
        prod.SetLastUpdatedToNow(DateTimeOffset.UtcNow);

        await context.SaveChangesAsync();
        return Result.Ok(MapToDto(entity));
    }

    public async Task<Result<PlayerDto>> BuildShipsAsync(int playerId, int count, ShipType type, int planetId)
    {
        if (count <= 0)
            return Result.Fail("Must build at least 1 ship");

        var planet = await QueryPlanet().FirstOrDefaultAsync(p => p.Id == planetId);
        if (planet == null) return Result.Fail("Planet not found");

        ResourceCalculator.Recalculate(planet);

        var player = await context.Players.FirstOrDefaultAsync(p => p.Id == playerId);
        if (player is null)
            return Result.Fail("Player not found");


        const int costPerShip = 10;
        var totalCost = count * costPerShip;

        var metal = planet.Resources.FirstOrDefault(r => r.Type == ResourceType.Metal);
        if (metal == null || metal.Amount < totalCost)
            return Result.Fail($"Not enough Metal. Need {totalCost}, have {metal?.Amount ?? 0}.");

        if (metal.Amount < totalCost)
            return Result.Fail($"Not enough Metal. Need {totalCost}, have {metal.Amount}.");
        
        var fleet = await context.Fleets.FirstOrDefaultAsync(p => p.PlayerId == player.Id && p.Type == type);

        metal.Amount -= totalCost;

        if (fleet == null)
        {
            fleet = new FleetEntity { PlayerId = player.Id, Type = type, Count = count };
            context.Fleets.Add(fleet);
        }
        else
        {
            fleet.Count += count;
        }
    
        await context.SaveChangesAsync();

        return Result.Ok(new PlayerDto { Id = player.Id, Name = player.Name });
    }

    private async Task<Result<PlanetDto>> RecalculateAndSaveAsync(PlanetEntity entity)
    {
        ResourceCalculator.Recalculate(entity);
        await context.SaveChangesAsync();
        return Result.Ok(MapToDto(entity));
    }

    private IQueryable<PlanetEntity> QueryPlanet() =>
        context.Planets.Include(p => p.Resources).Include(p => p.Productions);

    private static PlanetDto MapToDto(PlanetEntity e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Population = e.Population,
        Resources = e.Resources.Select(r => new ResourceDto { Type = r.Type, Amount = r.Amount }).ToList(),
        ResourceProductions = e.Productions.Select(resourceProductionDto).ToList()
    };

    private static ResourceProductionDto resourceProductionDto(ProductionEntity entity)
    {
        return new ResourceProductionDto
        {
            Type = entity.Type,
            Level = entity.Level,
            LastUpdated = entity.LastUpdated,
            PerMinute = entity.PerMinute,
            AmountPerHour = entity.AmountPerHour,
            MetalToUpgrade = entity.MetalToUpgrade,
            EnergyConsumption = entity.EnergyConsumption,
            EnergyNeededForUpgrade = entity.EnergyNeededForUpgrade
        };
    }

    internal async Task<IEnumerable<PlanetLookupDto>> ListPlanetsAsync()
    {
        var planets = await context.Planets.Select(p => new PlanetLookupDto { Id = p.Id, Name = p.Name }).ToListAsync();
        return planets;
    }

}