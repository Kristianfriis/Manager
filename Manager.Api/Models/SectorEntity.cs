using System.ComponentModel.DataAnnotations.Schema;
using Manager.Api.Data;
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

    public List<FleetEntity> Fleets { get; set; } = [];
}

public class FleetEntity
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public ShipType Type { get; set; }
    public int Count { get; set; }

    // Navigation properties
    public PlayerEntity? Player { get; set; }

    [NotMapped]
    public ShipStats Stats => GameRules.Ships[Type];
}

public class FleetMovementEntity
{
    public int Id { get; set; }
    public string PlayerName { get; set; } = "";
    public int ToZoneId { get; set; }
    public int ShipCount { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset ArrivalTime { get; set; }
    public bool IsClaim { get; set; }
    public bool Resolved { get; set; }
    public bool? AttackerWon { get; set; }
    public int? RemainingAttackerShips { get; set; }
    public int? ShipsLost { get; set; }
    public string? NewOwner { get; set; }

    public Dictionary<ShipType, int> ShipsMoving { get; set; } = [];
}
