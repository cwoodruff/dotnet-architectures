using Chinook.Orleans.Api.Data;
using Chinook.Orleans.Grains.Abstractions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var chinookConnection = builder.Configuration.GetConnectionString("Chinook")
    ?? throw new InvalidOperationException("Connection string 'Chinook' is not configured.");
var orleansConnection = builder.Configuration.GetConnectionString("ChinookOrleans")
    ?? throw new InvalidOperationException("Connection string 'ChinookOrleans' is not configured.");

builder.Services.AddOpenApi();
builder.Services.AddDbContext<ChinookDbContext>(options => options.UseSqlServer(chinookConnection));

// This process is an Orleans client, not a silo. It finds the silos through the same
// ADO.NET membership table they write to, so the silo must be running first.
builder.UseOrleansClient(client =>
{
    client.Configure<global::Orleans.Configuration.ClusterOptions>(options =>
    {
        options.ClusterId = builder.Configuration["Orleans:ClusterId"] ?? "chinook";
        options.ServiceId = builder.Configuration["Orleans:ServiceId"] ?? "chinook-orleans";
    });
    client.UseAdoNetClustering(options =>
    {
        options.Invariant = "Microsoft.Data.SqlClient";
        options.ConnectionString = orleansConnection;
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Reads Chinook directly with EF Core. Not every read needs to go through a grain.
app.MapGet("/artists", async (ChinookDbContext db, CancellationToken cancellationToken) =>
    await db.Artists
        .OrderBy(a => a.Name)
        .Take(10)
        .Select(a => new ArtistSummary(a.Id, a.Name, a.Albums.Count))
        .ToListAsync(cancellationToken));

// One call across the actor boundary: API to silo to the playlist's grain activation.
app.MapGet("/playlists/{id:int}/ping", async (int id, IGrainFactory grains) =>
    await grains.GetGrain<IPlaylistGrain>(id).Ping());

app.Run();

internal sealed record ArtistSummary(int Id, string? Name, int AlbumCount);
