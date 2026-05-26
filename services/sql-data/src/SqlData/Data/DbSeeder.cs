using Microsoft.EntityFrameworkCore;

namespace IndustrialPress.SqlData.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IndustrialDbContext db, CancellationToken cancellationToken = default)
    {
        if (await db.Sensors.AnyAsync(cancellationToken))
            return;

        for (var id = 1; id <= 20; id++)
        {
            db.Sensors.Add(new SensorEntity
            {
                Name = $"Sensor-{id:D2}",
                Location = "Line-1",
                Type = "temperature",
                Enabled = true
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
