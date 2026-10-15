using Chinook.VerticalSlice.Api.Data;
using Chinook.VerticalSlice.Api.Features.Artists;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Chinook")
    ?? throw new InvalidOperationException("Connection string 'Chinook' is not configured.");

builder.Services.AddOpenApi();
builder.Services.AddDbContext<ChinookDbContext>(options => options.UseSqlServer(connectionString));

// Each slice registers itself. Adding a feature means adding a file and one line here.
builder.Services.AddGetTopArtists();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGetTopArtists();

app.Run();
