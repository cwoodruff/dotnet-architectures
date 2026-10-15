using Chinook.ModMono.Modules.Catalog.Data;
using Chinook.ModMono.Modules.Catalog.Features;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Chinook.ModMono.Modules.Catalog;

/// <summary>
/// The Catalog module owns Artist, Album, Track, Genre, and MediaType. Nothing outside this
/// project touches those tables; other modules ask Catalog through its endpoints or, later,
/// through a public contract this module chooses to expose.
/// </summary>
public static class CatalogModule
{
    public static IServiceCollection AddCatalogModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Chinook")
            ?? throw new InvalidOperationException("Connection string 'Chinook' is not configured.");
        services.AddDbContext<CatalogDbContext>(options => options.UseSqlServer(connectionString));
        return services;
    }

    public static IEndpointRouteBuilder MapCatalogEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/artists", GetTopArtists.Handle).WithTags("Catalog");
        return endpoints;
    }
}
