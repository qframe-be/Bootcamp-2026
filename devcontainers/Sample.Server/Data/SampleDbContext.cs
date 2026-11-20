using Microsoft.EntityFrameworkCore;

namespace Sample.Server.Data;

public class SampleDbContext(DbContextOptions<SampleDbContext> options) : DbContext(options)
{
    public DbSet<WeatherForecast> Forecasts => Set<WeatherForecast>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WeatherForecast>(forecast =>
        {
            forecast.Property(f => f.Summary).HasMaxLength(50);
        });
    }
}
