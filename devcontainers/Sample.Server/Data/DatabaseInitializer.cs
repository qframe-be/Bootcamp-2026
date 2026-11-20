using Microsoft.EntityFrameworkCore;

namespace Sample.Server.Data;

public static class DatabaseInitializer
{
    public static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    // Creates the database and its tables when they don't exist yet, then seeds a few forecasts.
    // This keeps the sample free of migrations: an empty SQL Server (local or a fresh container) just works.
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<SampleDbContext>>();

        // Run inside the execution strategy so transient connection errors are retried,
        // e.g. when SQL Server is still starting up.
        await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            if (await db.Database.EnsureCreatedAsync())
            {
                logger.LogInformation("Created database {Database}", db.Database.GetDbConnection().Database);
            }

            if (!await db.Forecasts.AnyAsync())
            {
                var today = DateOnly.FromDateTime(DateTime.Now);
                db.Forecasts.AddRange(Enumerable.Range(1, 5).Select(index => CreateRandomForecast(today.AddDays(index))));
                await db.SaveChangesAsync();
                logger.LogInformation("Seeded the database with sample forecasts");
            }
        });
    }

    public static WeatherForecast CreateRandomForecast(DateOnly date) => new()
    {
        Date = date,
        TemperatureC = Random.Shared.Next(-20, 55),
        Summary = Summaries[Random.Shared.Next(Summaries.Length)]
    };
}
