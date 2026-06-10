namespace Manager.Shared.Dtos;

public class SectorDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int ZoneCount { get; set; }
    public int ClaimedCount { get; set; }
}

public class SectorDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public List<ZoneDto> Zones { get; set; } = [];
}

public class ZoneDto
{
    public int Id { get; set; }
    public int SectorId { get; set; }
    public int Ring { get; set; }
    public int Position { get; set; }
    public ResourceType BoostType { get; set; }
    public double BoostPercentage { get; set; }
    public string? OwnerName { get; set; }
    public int ShipCount { get; set; }
    public bool IsClaimed => OwnerName is not null;
}

public class PlayerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int ShipCount { get; set; }
    public int Metal { get; set; }
}

public class AttackResultDto
{
    public bool AttackerWon { get; set; }
    public int RemainingAttackerShips { get; set; }
    public int ShipsLost { get; set; }
    public string? NewOwner { get; set; }
}

public class FleetMovementDto
{
    public int Id { get; set; }
    public string PlayerName { get; set; } = "";
    public int ToZoneId { get; set; }
    public int Ring { get; set; }
    public int Position { get; set; }
    public int ShipCount { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset ArrivalTime { get; set; }
    public List<FleetCompositionDto> FleetComposition { get; set; } = [];
    public bool IsClaim { get; set; }
    public bool Resolved { get; set; }
    public bool? AttackerWon { get; set; }
    public int? RemainingAttackerShips { get; set; }
    public int? ShipsLost { get; set; }
    public string? NewOwner { get; set; }
    public bool HasArrived => DateTimeOffset.UtcNow >= ArrivalTime;
    public bool IsEnRoute => !HasArrived && !Resolved;
    public TimeSpan TimeUntilArrival => ArrivalTime - DateTimeOffset.UtcNow;
}

public record CreatePlanetRequest(string Name);
public record UpgradeRequest(ResourceType ResourceType);
public record ClaimRequest(int PlayerId, List<FleetCompositionDto> Fleet);
public record AttackRequest(string PlayerName, int ShipCount);
public record BuildShipsRequest(int Count, ShipTypeDto ShipType, int PlanetId, int PlayerId);
public record FleetCompositionDto(ShipTypeDto Type, int Count);

public record ShipStatsDto(
    ShipTypeDto Type,
    string Name,
    int BaseHealth,
    int AttackDamage,
    double Speed,
    int MetalCost
);

public enum ShipTypeDto
{
    Fighter,
    Bomber,
    Cruiser,
    Destroyer,
    Carrier
}

public class FleetDto
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public ShipTypeDto Type { get; set; }
    public int Count { get; set; }
    public ShipStatsDto? Stats { get; set; }
}