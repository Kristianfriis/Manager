using Manager.Api.Data;
using Manager.Api.Models;
using Manager.Shared.Dtos;

namespace Manager.Api.Mappers;

public static class FleetMapper
{
    public static FleetDto ToDto(this FleetEntity fleet)
    {
        return new FleetDto
        {
            Id = fleet.Id,
            PlayerId = fleet.PlayerId,
            Type = (ShipTypeDto)fleet.Type,
            Count = fleet.Count,
            Stats = MapToDto(fleet.Type)
        };
    }

    private static ShipStatsDto MapToDto(ShipType type)
    {
        var stats = GameRules.Ships[type];
        return new ShipStatsDto((ShipTypeDto)type, stats.Name, stats.BaseHealth, stats.AttackDamage, stats.Speed, stats.MetalCost);
    }
}