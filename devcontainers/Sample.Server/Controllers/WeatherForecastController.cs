using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sample.Server.Data;

namespace Sample.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController(SampleDbContext db, ILogger<WeatherForecastController> logger) : ControllerBase
{
    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<ActionResult<IEnumerable<WeatherForecast>>> Get()
    {
        try
        {
            return await db.Forecasts
                .OrderBy(f => f.Date)
                .ThenBy(f => f.Id)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            return DatabaseUnavailable(ex);
        }
    }

    // Adds a random forecast for the day after the last one, so you can see data being written
    // and check whether it survives restarting the containers.
    [HttpPost(Name = "AddWeatherForecast")]
    public async Task<ActionResult<WeatherForecast>> Post()
    {
        try
        {
            var lastDate = await db.Forecasts.MaxAsync(f => (DateOnly?)f.Date)
                ?? DateOnly.FromDateTime(DateTime.Now);

            var forecast = DatabaseInitializer.CreateRandomForecast(lastDate.AddDays(1));
            db.Forecasts.Add(forecast);
            await db.SaveChangesAsync();

            return forecast;
        }
        catch (Exception ex)
        {
            return DatabaseUnavailable(ex);
        }
    }

    private ObjectResult DatabaseUnavailable(Exception ex)
    {
        logger.LogError(ex, "Could not reach the database");
        return Problem(title: "Database not available", detail: DatabaseErrors.Describe(ex), statusCode: StatusCodes.Status503ServiceUnavailable);
    }
}
