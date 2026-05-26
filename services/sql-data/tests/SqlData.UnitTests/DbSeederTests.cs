using IndustrialPress.SqlData.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IndustrialPress.SqlData.UnitTests;

public class DbSeederTests
{
    private static IndustrialDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<IndustrialDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new IndustrialDbContext(options);
    }

    [Fact]
    public async Task SeedAsync_creates_exactly_twenty_sensors_with_ids_1_to_20()
    {
        await using var db = CreateDb();
        await DbSeeder.SeedAsync(db);

        var sensors = await db.Sensors.OrderBy(s => s.Id).ToListAsync();
        Assert.Equal(20, sensors.Count);
        Assert.Equal(Enumerable.Range(1, 20), sensors.Select(s => s.Id));
    }

    [Fact]
    public async Task SeedAsync_is_idempotent()
    {
        await using var db = CreateDb();
        await DbSeeder.SeedAsync(db);
        await DbSeeder.SeedAsync(db);

        Assert.Equal(20, await db.Sensors.CountAsync());
    }
}
