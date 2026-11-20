using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sample.Server.Data;

namespace Sample.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController(SampleDbContext db) : ControllerBase
{
    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<IEnumerable<WeatherForecast>> Get()
    {
        return await db.Forecasts
            .OrderBy(f => f.Date)
            .ThenBy(f => f.Id)
            .ToListAsync();
    }

    // Adds a random forecast for the day after the last one, so you can see data being written
    // and check whether it survives restarting the containers.
    [HttpPost(Name = "AddWeatherForecast")]
    public async Task<WeatherForecast> Post()
    {
        var lastDate = await db.Forecasts.MaxAsync(f => (DateOnly?)f.Date)
            ?? DateOnly.FromDateTime(DateTime.Now);

        var forecast = DatabaseInitializer.CreateRandomForecast(lastDate.AddDays(1));
        db.Forecasts.Add(forecast);
        await db.SaveChangesAsync();

        return forecast;
    }
}
