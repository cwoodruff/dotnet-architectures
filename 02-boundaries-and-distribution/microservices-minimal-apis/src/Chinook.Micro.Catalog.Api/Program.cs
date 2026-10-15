using Chinook.Micro.Catalog.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();

// Under the AppHost, ConnectionStrings:Chinook arrives as an environment variable from
// WithReference(chinook). Run on its own, it comes from appsettings.Development.json.
var connectionString = builder.Configuration.GetConnectionString("Chinook")
    ?? throw new InvalidOperationException("Connection string 'Chinook' is not configured.");
builder.Services.AddDbContext<ChinookDbContext>(options => options.UseSqlServer(connectionString));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/", () => Results.Ok(new { service = "Catalog" }));

app.MapGet("/artists", async (ChinookDbContext db, CancellationToken cancellationToken) =>
    await db.Artists
        .AsNoTracking()
        .OrderBy(a => a.Name)
        .Take(10)
        .Select(a => new ArtistSummary(a.Id, a.Name, a.Albums.Count))
        .ToListAsync(cancellationToken));

app.MapDefaultEndpoints();

app.Run();

public sealed record ArtistSummary(int Id, string? Name, int AlbumCount);
