using Manager.Api.Models;
using Manager.Shared.Dtos;

namespace Manager.Api.Mappers;

public static class ZoneMapper
{
    public static SectorDetailDto MapToDetail(this SectorEntity e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Zones = e.Zones.Select(MapZoneToDto).ToList()
    };

    public static ZoneDto MapZoneToDto(this ZoneEntity z) => new()
    {
        Id = z.Id,
        SectorId = z.SectorId,
        Ring = z.Ring,
        Position = z.Position,
        BoostType = z.BoostType,
        BoostPercentage = z.BoostPercentage,
        OwnerId = z.OwnerId,
        OwnerName = z.Owner?.Name,
        ShipCount = z.ShipCount,
        FleetComposition = z.ShipsInZone.Select(kvp => new FleetCompositionDto((ShipTypeDto)kvp.Key, kvp.Value)).ToArray()
    };

    public static FleetMovementDto MapToMovementDto(this FleetMovementEntity m, int ring, int pos) => new()
    {
        Id = m.Id,
        PlayerId = m.PlayerId,
        PlayerName = m.PlayerName,
        ToZoneId = m.ToZoneId,
        Ring = ring,
        Position = pos,
        ShipCount = m.ShipCount,
        StartTime = m.StartTime,
        ArrivalTime = m.ArrivalTime,
        IsClaim = m.IsClaim,
        IsReinforce = m.IsReinforce,
        Resolved = m.Resolved,
        AttackerWon = m.AttackerWon,
        RemainingAttackerShips = m.RemainingAttackerShips,
        ShipsLost = m.ShipsLost,
        NewOwner = m.NewOwner,
        FleetComposition = m.ShipsMoving.Select(kvp => new FleetCompositionDto((ShipTypeDto)kvp.Key, kvp.Value)).ToList()
    };
}