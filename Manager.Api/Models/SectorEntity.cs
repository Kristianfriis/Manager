using Manager.Shared.Dtos;

namespace Manager.Api.Models;

public class SectorEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public List<ZoneEntity> Zones { get; set; } = [];
}

public class ZoneEntity
{
    public int Id { get; set; }
    public int SectorId { get; set; }
    public int Ring { get; set; }
    public int Position { get; set; }
    public ResourceType BoostType { get; set; }
    public double BoostPercentage { get; set; }
    public string? OwnerName { get; set; }
    public int ShipCount { get; set; }
}

public class PlayerEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int ShipCount { get; set; }
    public int Metal { get; set; }
}
