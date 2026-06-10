namespace Manager.Api.Models;

public class FleetMovement
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public int ZoneId { get; set; }
    public List<FleetComposition> FleetComposition { get; set; } = new();
    public DateTimeOffset ArrivalTime { get; set; }
}

public record FleetComposition(ShipType Type, int Count);