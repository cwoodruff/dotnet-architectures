using Chinook.Micro.Playlists.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();

// Same Chinook database as the other services for now. Giving this service its own data
// is use-case work; see the README.
var connectionString = builder.Configuration.GetConnectionString("Chinook")
    ?? throw new InvalidOperationException("Connection string 'Chinook' is not configured.");
builder.Services.AddDbContext<ChinookDbContext>(options => options.UseSqlServer(connectionString));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/", () => Results.Ok(new { service = "Playlists" }));

app.MapDefaultEndpoints();

app.Run();
