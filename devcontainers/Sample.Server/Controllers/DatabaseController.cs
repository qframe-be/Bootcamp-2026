using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sample.Server.Data;

namespace Sample.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class DatabaseController(SampleDbContext db) : ControllerBase
{
    // Reports which SQL Server the app is talking to, so you can tell a local instance
    // apart from the one running in a container.
    [HttpGet(Name = "GetDatabaseInfo")]
    public async Task<DatabaseInfo> Get()
    {
        var info = await db.Database
            .SqlQuery<DatabaseInfo>($"SELECT CAST(@@SERVERNAME AS nvarchar(128)) AS Server, DB_NAME() AS [Database], CAST(@@VERSION AS nvarchar(4000)) AS Version")
            .SingleAsync();

        // @@VERSION spans several lines; the first one names the product and version.
        return info with { Version = info.Version.Split('\n')[0].Trim() };
    }
}

public record DatabaseInfo
{
    public string Server { get; init; } = "";
    public string Database { get; init; } = "";
    public string Version { get; init; } = "";
}
