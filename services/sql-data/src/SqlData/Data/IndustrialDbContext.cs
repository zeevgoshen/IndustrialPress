using Microsoft.EntityFrameworkCore;

namespace IndustrialPress.SqlData.Data;

public sealed class IndustrialDbContext(DbContextOptions<IndustrialDbContext> options) : DbContext(options)
{
    public DbSet<SensorEntity> Sensors => Set<SensorEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SensorEntity>(e =>
        {
            e.ToTable("Sensors");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.Name).HasMaxLength(64).IsRequired();
            e.Property(x => x.Location).HasMaxLength(128).IsRequired();
            e.Property(x => x.Type).HasMaxLength(32).IsRequired();
        });
    }
}
