using System;

namespace Manager.Shared.Dtos;

public class PlanetDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "Unnamed Planet";
    public int Population { get; set; }
    public List<ResourceDto> Resources { get; set; } = [];
    public List<ResourceProductionDto> ResourceProductions { get; set; } = [];
}

public class ResourceDto
{
    public ResourceType Type { get; set; }
    public int Amount { get; set; }
}

public enum ResourceType
{
    None,
    Food,
    Water,
    Metal,
    Energy
}

public class ResourceProductionDto
{
    public ResourceType Type { get; set; }
    public DateTimeOffset LastUpdated { get; set; }
    public int Level { get; set; }
    public int AmountPerHour { get; set; }
    public double PerMinute { get; set; }
    public long MetalToUpgrade { get; set; }
    public long EnergyConsumption { get; set; }
    public long EnergyNeededForUpgrade { get; set; }
}

public class PlanetLookupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "Unnamed Planet";
}