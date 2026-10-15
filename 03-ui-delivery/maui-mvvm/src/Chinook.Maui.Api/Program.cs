using Chinook.Maui.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddDbContext<ChinookDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Chinook")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// The native app calls this API over plain HTTP in development (the Android emulator reaches the host at
// 10.0.2.2), so there is no HTTPS redirect here. A deployed API would sit behind TLS.
app.MapGet("/artists", async (ChinookDbContext db, CancellationToken cancellationToken) =>
    await db.Artists
        .AsNoTracking()
        .OrderBy(a => a.Name)
        .Take(10)
        .Select(a => new ArtistSummary(a.Id, a.Name ?? "(unnamed)", a.Albums.Count))
        .ToListAsync(cancellationToken))
    .WithName("GetTopArtists");

app.Run();

internal sealed record ArtistSummary(int Id, string Name, int AlbumCount);
