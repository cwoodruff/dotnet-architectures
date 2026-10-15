using Chinook.BlazorAuto.Web.Artists;
using Chinook.BlazorAuto.Web.Client.Artists;
using Chinook.BlazorAuto.Web.Components;
using Chinook.BlazorAuto.Web.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// AddDbContextFactory also registers a scoped ChinookDbContext, which the minimal API endpoint uses.
builder.Services.AddDbContextFactory<ChinookDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Chinook")));
builder.Services.AddScoped<IArtistsClient, ServerArtistsClient>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapArtistsApi();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Chinook.BlazorAuto.Web.Client._Imports).Assembly);

app.Run();
