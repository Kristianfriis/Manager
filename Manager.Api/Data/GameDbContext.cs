using Microsoft.EntityFrameworkCore;
using Manager.Api.Models;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Manager.Api.Data;

public class GameDbContext : DbContext
{
    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options) { }

    public DbSet<PlanetEntity> Planets => Set<PlanetEntity>();
    public DbSet<ResourceEntity> Resources => Set<ResourceEntity>();
    public DbSet<ProductionEntity> Productions => Set<ProductionEntity>();
    public DbSet<SectorEntity> Sectors => Set<SectorEntity>();
    public DbSet<ZoneEntity> Zones => Set<ZoneEntity>();
    public DbSet<PlayerEntity> Players => Set<PlayerEntity>();
    public DbSet<FleetEntity> Fleets => Set<FleetEntity>();
    public DbSet<FleetMovementEntity> FleetMovements => Set<FleetMovementEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PlanetEntity>(e =>
        {
            e.HasMany(p => p.Resources).WithOne().HasForeignKey(r => r.PlanetId);
            e.HasMany(p => p.Productions).WithOne().HasForeignKey(p => p.PlanetId);
        });

        modelBuilder.Entity<SectorEntity>(e =>
        {
            e.HasMany(s => s.Zones).WithOne().HasForeignKey(z => z.SectorId);
        });

        modelBuilder.Entity<FleetEntity>(e =>
        {
            e.HasOne(f => f.Player)
                .WithMany(p => p.Fleets)
                .HasForeignKey(f => f.PlayerId);

            e.Property(f => f.Type)
                .HasConversion<string>();

            e.HasIndex(f => new { f.PlayerId, f.Type })
                .IsUnique();
        });

        modelBuilder.Entity<FleetMovementEntity>(e =>
        {
            e.Property(f => f.ShipsMoving)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<ShipType, int>>(v, (JsonSerializerOptions?)null)!);
        });

        var comparer = new ValueComparer<Dictionary<ShipType, int>>(
        (d1, d2) =>
            d1!.Count == d2!.Count &&
            d1.OrderBy(x => x.Key)
            .SequenceEqual(d2.OrderBy(x => x.Key)),

        d => d.Aggregate(
            0,
            (a, v) => HashCode.Combine(a, v.Key, v.Value)),

        d => d.ToDictionary(entry => entry.Key, entry => entry.Value)
    );

        modelBuilder.Entity<ZoneEntity>(e =>
        {
            e.HasOne(z => z.Owner)
                .WithMany()
                .HasForeignKey(z => z.OwnerId)
                .IsRequired(false);

            // Tells EF Core to serialize the Dictionary into a single database column named "ShipsInZone"
            e.Property(x => x.ShipsInZone)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Dictionary<ShipType, int>>(v, (JsonSerializerOptions?)null)!)
            .Metadata.SetValueComparer(comparer);
        });
    }
}
