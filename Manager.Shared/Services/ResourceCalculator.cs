using Manager.Shared.Dtos;

namespace Manager.Shared.Services;

public static class ResourceCalculator
{
    public static void Recalculate(PlanetDto planet)
    {
        var now = DateTimeOffset.UtcNow;

        foreach (var prod in planet.ResourceProductions)
        {
            if(prod.Level <= 0) continue;
            
            var elapsed = now - prod.LastUpdated;
            if (elapsed.TotalSeconds < 1) continue;

            var gained = prod.AmountPerHour * elapsed.TotalHours;

            var resource = planet.Resources
                .FirstOrDefault(r => r.Type == prod.Type);

            if (resource != null)
                resource.Amount = (int)(resource.Amount + gained);
            else
                planet.Resources.Add(new ResourceDto
                    { Type = prod.Type, Amount = (int)gained });

            prod.LastUpdated = now;
        }
    }
}