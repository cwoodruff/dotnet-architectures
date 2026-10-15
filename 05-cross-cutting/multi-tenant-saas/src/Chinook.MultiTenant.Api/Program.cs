using Chinook.MultiTenant.Api.Data;
using Chinook.MultiTenant.Api.Features;
using Chinook.MultiTenant.Api.Tenancy;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddSingleton<ITenantRegistry, ConfigurationTenantRegistry>();
builder.Services.AddScoped<TenantContext>();
builder.Services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());

// Database per tenant: the options are built per request scope, from whichever tenant the
// middleware resolved. The model is the same for every tenant, so EF Core's model cache is shared.
builder.Services.AddDbContext<ChinookDbContext>((sp, options) =>
{
    var tenant = sp.GetRequiredService<ITenantContext>().Current
        ?? throw new InvalidOperationException("ChinookDbContext was requested before a tenant was resolved.");
    options.UseSqlServer(tenant.ConnectionString);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Must run before routing so a path-based tenant prefix can be moved into PathBase.
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseRouting();

app.MapArtistEndpoints();

app.Run();
