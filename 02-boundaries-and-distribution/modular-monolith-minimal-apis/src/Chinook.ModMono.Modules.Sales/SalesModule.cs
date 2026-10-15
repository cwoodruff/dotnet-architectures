using Chinook.ModMono.Modules.Sales.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Chinook.ModMono.Modules.Sales;

/// <summary>
/// The Sales module owns Customer, Invoice, InvoiceLine, and Employee. Its endpoints are placeholders until the use cases land.
/// </summary>
public static class SalesModule
{
    public static IServiceCollection AddSalesModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Chinook")
            ?? throw new InvalidOperationException("Connection string 'Chinook' is not configured.");
        services.AddDbContext<SalesDbContext>(options => options.UseSqlServer(connectionString));
        return services;
    }

    public static IEndpointRouteBuilder MapSalesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/sales", () => Results.Ok(new { module = "Sales" })).WithTags("Sales");
        return endpoints;
    }
}
