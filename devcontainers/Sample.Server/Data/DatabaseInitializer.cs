using Microsoft.EntityFrameworkCore;

namespace Sample.Server.Data;

// Creates the database and its tables when they don't exist yet, then seeds a few forecasts.
// This keeps the sample free of migrations: an empty SQL Server (local or a fresh container) just works.
//
// It runs in the background so the app starts even when the database isn't reachable yet,
// and keeps retrying until it is (e.g. while a SQL Server container is still starting up).
public class DatabaseInitializer(IServiceProvider services, ILogger<DatabaseInitializer> logger) : BackgroundService
{
    public static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    private static readonly TimeSpan MaxRetryDelay = TimeSpan.FromSeconds(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await InitializeAsync(stoppingToken);
                return;
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                var delay = TimeSpan.FromSeconds(Math.Min(Math.Pow(2, attempt), MaxRetryDelay.TotalSeconds));
                logger.LogWarning(
                    "Database is not available (attempt {Attempt}), retrying in {Delay}s: {Error}",
                    attempt, delay.TotalSeconds, DatabaseErrors.Describe(ex));
                await Task.Delay(delay, stoppingToken);
            }
        }
    }

    private async Task InitializeAsync(CancellationToken cancellationToken)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SampleDbContext>();

        if (await db.Database.EnsureCreatedAsync(cancellationToken))
        {
            logger.LogInformation("Created database {Database}", db.Database.GetDbConnection().Database);
        }

        if (!await db.Forecasts.AnyAsync(cancellationToken))
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            db.Forecasts.AddRange(Enumerable.Range(1, 5).Select(index => CreateRandomForecast(today.AddDays(index))));
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Seeded the database with sample forecasts");
        }

        logger.LogInformation("Database is ready");
    }

    public static WeatherForecast CreateRandomForecast(DateOnly date) => new()
    {
        Date = date,
        TemperatureC = Random.Shared.Next(-20, 55),
        Summary = Summaries[Random.Shared.Next(Summaries.Length)]
    };
}
