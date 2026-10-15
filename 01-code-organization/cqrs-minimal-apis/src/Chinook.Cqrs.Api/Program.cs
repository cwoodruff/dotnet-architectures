using Chinook.Cqrs.Api.Commands;
using Chinook.Cqrs.Api.Data;
using Chinook.Cqrs.Api.Dispatching;
using Chinook.Cqrs.Api.Endpoints;
using Chinook.Cqrs.Api.Queries;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Chinook")
    ?? throw new InvalidOperationException("Connection string 'Chinook' is not configured.");

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

// Write side: EF Core.
builder.Services.AddDbContext<ChinookDbContext>(options => options.UseSqlServer(connectionString));

// Read side: Dapper over plain connections.
builder.Services.AddSingleton(new SqlConnectionFactory(connectionString));

// Dispatcher and handlers, registered by hand so every handler is visible here.
builder.Services.AddScoped<Dispatcher>();
builder.Services.AddScoped<IQueryHandler<GetTopArtistsQuery, IReadOnlyList<TopArtist>>, GetTopArtistsQueryHandler>();
builder.Services.AddScoped<ICommandHandler<CreatePlaylistCommand, int>, CreatePlaylistCommandHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapArtistEndpoints();
app.MapPlaylistEndpoints();

app.Run();
