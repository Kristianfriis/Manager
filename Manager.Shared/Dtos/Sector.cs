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
