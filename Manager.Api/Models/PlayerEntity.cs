namespace Manager.Api.Models;

public class PlayerEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = "";

    public List<FleetEntity> Fleets { get; set; } = [];
    public List<PlanetEntity> Planets { get; set; } = [];
}
