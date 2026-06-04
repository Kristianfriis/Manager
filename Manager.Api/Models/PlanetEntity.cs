using Manager.Shared.Dtos;

namespace Manager.Api.Models;

public class PlanetEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = "Unnamed Planet";
    public int Population { get; set; }
    public List<ResourceEntity> Resources { get; set; } = [];
    public List<ProductionEntity> Productions { get; set; } = [];
  
    public PlanetEntity()
    {
    }

    public void New()
    {
        Productions.Add(new ProductionEntity(ResourceType.Metal));
        Productions.Add(new ProductionEntity(ResourceType.Energy));
        Productions.Add(new ProductionEntity(ResourceType.Food));
        Productions.Add(new ProductionEntity(ResourceType.Water));

        Resources.Add(new ResourceEntity { Type = ResourceType.Metal, Amount = 100 });
        Resources.Add(new ResourceEntity { Type = ResourceType.Energy, Amount = 100 });
        Resources.Add(new ResourceEntity { Type = ResourceType.Food, Amount = 0 });
        Resources.Add(new ResourceEntity { Type = ResourceType.Water, Amount = 0 });
    }
}

public class ResourceEntity
{
    public int Id { get; set; }
    public int PlanetId { get; set; }
    public ResourceType Type { get; set; }
    public int Amount { get; set; }
}

public class ProductionEntity
{
    public int Id { get; init; }
    public int PlanetId { get; set; }
    public ResourceType Type { get; set; }
    public int Level { get; set; }
    public DateTimeOffset LastUpdated { get; set; }
    private int BaseProduction { get; } = 0;
    public int AmountPerHour => Level * BaseProduction;
    public int PerMinute => (int)Math.Ceiling(AmountPerHour / 60.0);
    public long MetalToUpgrade => (long)Math.Pow(2, Level) * 100;
    public long EnergyConsumption => (long)Math.Pow(2, Level) * 50;
    public long EnergyNeededForUpgrade => (long)Math.Pow(2, Level) * 50;

    public void SetLastUpdatedToNow(DateTimeOffset now)
    {
        LastUpdated = new DateTimeOffset(
            now.Year, 
            now.Month, 
            now.Day, 
            now.Hour, 
            now.Minute, 
            0, 
            now.Offset
        );
    }

    public ProductionEntity()
    {
    }

    public ProductionEntity(ResourceType type)
    {
        Type = type;
        Level = 0;

        BaseProduction = type switch
        {
            ResourceType.Food => 20,
            ResourceType.Water => 15,
            ResourceType.Metal => 10,
            ResourceType.Energy => 5,
            _ => 0
        };
    }
}
