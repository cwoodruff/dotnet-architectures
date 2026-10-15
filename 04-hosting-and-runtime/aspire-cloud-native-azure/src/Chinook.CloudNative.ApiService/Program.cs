using Chinook.CloudNative.ApiService.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// The Aspire SQL Server EF Core integration reads ConnectionStrings:Chinook, which the AppHost
// supplies (and appsettings.Development.json supplies when the service runs on its own). It also
// adds health checks, retries, and tracing for the DbContext.
builder.AddSqlServerDbContext<ChinookDbContext>("Chinook");

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/", () => "Chinook API service is running. Try GET /artists.");

app.MapGet("/artists", async (ChinookDbContext db, CancellationToken ct) =>
    await db.Artists
        .OrderBy(a => a.Name)
        .Take(10)
        .Select(a => new ArtistSummary(a.Id, a.Name, a.Albums.Count))
        .ToListAsync(ct))
    .WithName("GetTopArtists");

app.MapDefaultEndpoints();

app.Run();

public sealed record ArtistSummary(int Id, string? Name, int AlbumCount);
