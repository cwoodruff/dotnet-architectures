using Chinook.EventSourcing.Api.Endpoints;
using Chinook.EventSourcing.Api.EventStore;
using Chinook.EventSourcing.Api.ReadModel;

var builder = WebApplication.CreateBuilder(args);

var chinook = builder.Configuration.GetConnectionString("Chinook")
    ?? throw new InvalidOperationException("Connection string 'Chinook' is not configured.");
var chinookEvents = builder.Configuration.GetConnectionString("ChinookEvents")
    ?? throw new InvalidOperationException("Connection string 'ChinookEvents' is not configured.");

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

// Write side: the event store in ChinookEvents.
builder.Services.AddSingleton<EventTypeMap>();
builder.Services.AddSingleton(sp => new SqlEventStore(chinookEvents, sp.GetRequiredService<EventTypeMap>()));

// Read side: Chinook itself, through Dapper.
builder.Services.AddSingleton(new ArtistQueries(chinook));

var app = builder.Build();

// Create ChinookEvents and its Events table if they are missing. Runtime only; the build never needs SQL Server.
await EventStoreSchema.EnsureCreatedAsync(chinookEvents);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapArtistEndpoints();
app.MapPlaylistEndpoints();

await app.RunAsync();
