using Microsoft.EntityFrameworkCore;
using Sample.Server.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// The connection string comes from configuration ("ConnectionStrings:SampleDb" in appsettings),
// and can be overridden with the environment variable ConnectionStrings__SampleDb.
builder.Services.AddDbContext<SampleDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SampleDb")));

// Creates and seeds the database in the background, retrying until it is reachable.
builder.Services.AddHostedService<DatabaseInitializer>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseDefaultFiles();
app.MapStaticAssets();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
