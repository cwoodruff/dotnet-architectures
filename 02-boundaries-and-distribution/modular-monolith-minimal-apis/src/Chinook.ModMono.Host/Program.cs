using Chinook.ModMono.Modules.Catalog;
using Chinook.ModMono.Modules.Playlists;
using Chinook.ModMono.Modules.Sales;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// The host knows the modules exist and nothing else. Each module registers its own DbContext.
builder.Services.AddCatalogModule(builder.Configuration);
builder.Services.AddSalesModule(builder.Configuration);
builder.Services.AddPlaylistsModule(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapCatalogEndpoints();
app.MapSalesEndpoints();
app.MapPlaylistsEndpoints();

app.Run();
