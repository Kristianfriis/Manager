using Manager.Api.Models;

namespace Manager.Api.Data;

public static class GameRules
{
    public static readonly Dictionary<ShipType, ShipStats> Ships = new()
    {
        { 
            ShipType.Fighter, 
            new ShipStats("Interceptor Fighter", BaseHealth: 100, AttackDamage: 15, Speed: 5, MetalCost: 50) 
        },
        { 
            ShipType.Bomber, 
            new ShipStats("Heavy Bomber", BaseHealth: 180, AttackDamage: 45, Speed: 3, MetalCost: 120) 
        },
        { 
            ShipType.Cruiser, 
            new ShipStats("Strike Cruiser", BaseHealth: 500, AttackDamage: 110, Speed: 4, MetalCost: 400) 
        },
        { 
            ShipType.Destroyer, 
            new ShipStats("Star Destroyer", BaseHealth: 1200, AttackDamage: 320, Speed: 2, MetalCost: 1100) 
        },
        { 
            ShipType.Carrier, 
            new ShipStats("Fleet Carrier", BaseHealth: 2500, AttackDamage: 50, Speed: 1, MetalCost: 2500) 
        }
    };
}