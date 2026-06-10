using Microsoft.EntityFrameworkCore;
using Manager.Api.Models;

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
            // Tells EF Core to serialize the Dictionary into a single database column named "ShipsMoving"
            e.OwnsOne(f => f.ShipsMoving, builder =>
            {
                builder.ToJson();
            });
        });
    }
}
