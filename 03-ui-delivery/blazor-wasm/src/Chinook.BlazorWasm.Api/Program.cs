using Chinook.BlazorWasm.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddDbContext<ChinookDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Chinook")));

// The WebAssembly client is served from a different origin (its own dev server, or a static host in
// production), so the browser only lets it call this API if the API allows that origin.
var clientOrigins = builder.Configuration.GetSection("Cors:ClientOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy => policy.WithOrigins(clientOrigins).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();

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
