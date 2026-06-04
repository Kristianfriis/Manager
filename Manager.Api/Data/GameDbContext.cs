using Microsoft.EntityFrameworkCore;
using Manager.Api.Models;

namespace Manager.Api.Data;

public class GameDbContext : DbContext
{
    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options) { }

    public DbSet<PlanetEntity> Planets => Set<PlanetEntity>();
    public DbSet<ResourceEntity> Resources => Set<ResourceEntity>();
    public DbSet<ProductionEntity> Productions => Set<ProductionEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PlanetEntity>(e =>
        {
            e.HasMany(p => p.Resources).WithOne().HasForeignKey(r => r.PlanetId);
            e.HasMany(p => p.Productions).WithOne().HasForeignKey(p => p.PlanetId);
        });
    }
}
