using Manager.Shared.Dtos;

namespace Manager.Shared.Services.Tests;

public class ResourceCalculatorTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void BasicResourceCalculation()
    {
        var ti = new PlanetDto
        {
            Id = 1,
            Name = "Test Planet",
            Population = 1000,
            Resources = new List<ResourceDto>
            {
                new ResourceDto { Type = ResourceType.Food, Amount = 500 },
                new ResourceDto { Type = ResourceType.Water, Amount = 300 }
            },
            ResourceProductions = new List<ResourceProductionDto>
            {
                new ResourceProductionDto
                {
                    Type = ResourceType.Food,
                    LastUpdated = DateTimeOffset.UtcNow.AddHours(-1),
                    Level = 2
                },
                new ResourceProductionDto
                {
                    Type = ResourceType.Water,
                    LastUpdated = DateTimeOffset.UtcNow.AddHours(-2),
                    Level = 3
                }
            }
        };

        ResourceCalculator.Recalculate(ti);

        var food = ti.Resources.First(r => r.Type == ResourceType.Food);
        var water = ti.Resources.First(r => r.Type == ResourceType.Water);

        Assert.That(food.Amount, Is.EqualTo(500 + 20));
        Assert.That(water.Amount, Is.EqualTo(300 + 60));
    }
}