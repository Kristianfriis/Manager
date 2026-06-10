using System.Text.Json;
using Manager.Api.Data;
using Manager.Api.Hubs;
using Manager.Api.Mappers;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Manager.Api;

public class ZoneBroadcastService : BackgroundService
{
    private readonly IHubContext<MovementHub> _hubContext;
    private readonly ILogger<ZoneBroadcastService> _logger;
    private readonly TimeSpan _period = TimeSpan.FromSeconds(10); // Broadcast every 10 seconds
    private readonly IServiceProvider _serviceProvider;

    public ZoneBroadcastService(IHubContext<MovementHub> hubContext, ILogger<ZoneBroadcastService> logger, IServiceProvider serviceProvider)
    {
        _hubContext = hubContext;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(_period);

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            // Create a scope per tick to safely use scoped services like EF DbContext
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<GameDbContext>();
                var activeSectors = await dbContext.Sectors
                    .Include(s => s.Zones)
                        .ThenInclude(z => z.Owner)
                    .ToListAsync(stoppingToken);

                var movements = await dbContext.FleetMovements
                    .Where(m => !m.Resolved)
                    .ToListAsync(stoppingToken);

                var now = DateTimeOffset.UtcNow;

                foreach (var sector in activeSectors)
                {
                    var updated = false;

                    foreach (var zone in sector.Zones)
                    {
                        var zoneMovements = movements.Where(m => m.ToZoneId == zone.Id).ToList();

                        if (zoneMovements.Count == 0)
                        {
                            _logger.LogInformation("No movements found for sector {SectorId}, zone {ZoneId}", sector.Id, zone.Id);
                            continue;
                        }

                        foreach (var movement in zoneMovements)
                        {

                            if (!movement.Resolved && now >= movement.ArrivalTime)
                            {
                                _logger.LogInformation("Resolving movement {MovementId} for player {PlayerName} to zone {ZoneId}", movement.Id, movement.PlayerName, zone.Id);
                                SectorService.ResolveMovement(movement, zone);
                                updated = true;

                                Console.WriteLine(JsonSerializer.Serialize(zone));
                            }
                        }
                    }

                    if (updated)
                    {
                        await dbContext.SaveChangesAsync(stoppingToken);

                        var sectorDto = sector.MapToDetail();

                        Console.WriteLine($"Broadcasting updates for sector {sector.Id} with {sector.Zones.Count} zones.");

                        await _hubContext.Clients.Group($"sector_{sector.Id}")
                            .SendAsync("MovementUpdated", sectorDto, stoppingToken);
                    }

                }
            }
        }
    }
}