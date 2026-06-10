namespace Manager.Api.Models;

public record ShipStats(
    string Name,
    int BaseHealth,
    int AttackDamage,
    double Speed,
    int MetalCost
);