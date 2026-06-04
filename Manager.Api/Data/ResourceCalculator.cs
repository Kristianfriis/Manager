using Manager.Api.Models;
using Manager.Shared.Dtos;

namespace Manager.Api.Data;

public static class ResourceCalculator
{
    public static void Recalculate(PlanetEntity planet)
    {
        var now = DateTimeOffset.UtcNow;
        // Snap the current time to the current whole minute (dropping seconds/milliseconds)
        var currentWholeMinute = new DateTimeOffset(
            now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, now.Offset
        );

        foreach (var prod in planet.Productions)
        {
            if (prod.LastUpdated == default)
            {
                prod.SetLastUpdatedToNow(currentWholeMinute);
                continue; // Skip this tick; it starts producing from this minute forward
            }

            // Calculate elapsed time based on the snapped whole minutes
            var elapsed = currentWholeMinute - prod.LastUpdated;

            // If a full minute hasn't passed since the last tracked whole minute, skip
            if (elapsed.TotalMinutes < 1) continue;

            if (prod.PerMinute <= 0) continue;

            // Since elapsed is snapped to whole minutes, TotalMinutes will be a clean integer (1.0, 2.0, etc.)
            var minutesGained = Math.Floor(elapsed.TotalMinutes);
            var gained = prod.PerMinute * minutesGained;

            var resource = planet.Resources
                .FirstOrDefault(r => r.Type == prod.Type);

            if (resource != null)
                resource.Amount += (int)gained;

            // Advance LastUpdated precisely by the number of whole minutes processed
            prod.SetLastUpdatedToNow(currentWholeMinute);
        }
    }
}