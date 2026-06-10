using Manager.Api.Models;
using Manager.Shared.Dtos;

namespace Manager.Api.Tests.Models;

public class ProductionEntityTests
{
    [Test]
    public void Metal_Should_Have_Correct_BaseProduction_Level_1()
    {
        var production = new ProductionEntity(ResourceType.Metal);
        production.Level = 1;
        Assert.That(production.BaseProduction, Is.EqualTo(5));
        Assert.That(production.PerMinute, Is.EqualTo(10));
        Assert.That(production.AmountPerHour, Is.EqualTo(600));
    }

    [Test]
    public void Metal_Should_Have_Correct_BaseProduction_Level_2()
    {
        var production = new ProductionEntity(ResourceType.Metal);
        production.Level = 2;
        Assert.That(production.BaseProduction, Is.EqualTo(5));
        Assert.That(production.PerMinute, Is.EqualTo(20));
        Assert.That(production.AmountPerHour, Is.EqualTo(1200));
    }

    [Test]
    public void Metal_Should_Have_Correct_BaseProduction_Level_3()
    {
        var production = new ProductionEntity(ResourceType.Metal);
        production.Level = 3;
        Assert.That(production.BaseProduction, Is.EqualTo(5));
        Assert.That(production.PerMinute, Is.EqualTo(40));
        Assert.That(production.AmountPerHour, Is.EqualTo(2400));
    }

    [Test]
    public void Energy_Should_Have_Correct_BaseProduction_Level_1()
    {
        var production = new ProductionEntity(ResourceType.Energy);
        production.Level = 1;
        Assert.That(production.BaseProduction, Is.EqualTo(6));
        Assert.That(production.PerMinute, Is.EqualTo(12));
        Assert.That(production.AmountPerHour, Is.EqualTo(720));
    }

    [Test]
    public void Food_Should_Have_Correct_BaseProduction_Level_1()
    {
        var production = new ProductionEntity(ResourceType.Food);
        production.Level = 1;
        Assert.That(production.BaseProduction, Is.EqualTo(2));
        Assert.That(production.PerMinute, Is.EqualTo(4));
        Assert.That(production.AmountPerHour, Is.EqualTo(240));    
    }

    [Test]
    public void Water_Should_Have_Correct_BaseProduction_Level_1()
    {
        var production = new ProductionEntity(ResourceType.Water);
        production.Level = 1;
        Assert.That(production.BaseProduction, Is.EqualTo(4));
        Assert.That(production.PerMinute, Is.EqualTo(8));
        Assert.That(production.AmountPerHour, Is.EqualTo(480));
    }
}