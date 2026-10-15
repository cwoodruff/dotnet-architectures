using Chinook.BlazorServer.Web.Components;
using Chinook.BlazorServer.Web.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Blazor Server components outlive a request scope, so they ask a factory for a short-lived context per
// operation instead of injecting a scoped ChinookDbContext. See Components/Pages/Artists.razor.
builder.Services.AddDbContextFactory<ChinookDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Chinook")));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
