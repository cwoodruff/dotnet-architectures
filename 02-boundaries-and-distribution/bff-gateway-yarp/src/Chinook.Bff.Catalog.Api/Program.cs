using Chinook.Bff.Catalog.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var connectionString = builder.Configuration.GetConnectionString("Chinook")
    ?? throw new InvalidOperationException("Connection string 'Chinook' is not configured.");

builder.Services.AddOpenApi();
builder.Services.AddDbContext<ChinookDbContext>(options => options.UseSqlServer(connectionString));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapDefaultEndpoints();

// Clients reach this through the gateway as GET /api/catalog/artists.
app.MapGet("/artists", async (ChinookDbContext db, CancellationToken cancellationToken) =>
    await db.Artists
        .OrderBy(a => a.Name)
        .Take(10)
        .Select(a => new ArtistSummary(a.Id, a.Name, a.Albums.Count))
        .ToListAsync(cancellationToken));

app.Run();

internal sealed record ArtistSummary(int Id, string? Name, int AlbumCount);
