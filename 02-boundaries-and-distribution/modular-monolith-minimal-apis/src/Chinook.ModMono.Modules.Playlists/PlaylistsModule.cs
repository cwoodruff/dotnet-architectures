using Chinook.ModMono.Modules.Playlists.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Chinook.ModMono.Modules.Playlists;

/// <summary>
/// The Playlists module owns Playlist and PlaylistTrack. Its endpoints are placeholders until the use cases land.
/// </summary>
public static class PlaylistsModule
{
    public static IServiceCollection AddPlaylistsModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Chinook")
            ?? throw new InvalidOperationException("Connection string 'Chinook' is not configured.");
        services.AddDbContext<PlaylistsDbContext>(options => options.UseSqlServer(connectionString));
        return services;
    }

    public static IEndpointRouteBuilder MapPlaylistsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/playlists", () => Results.Ok(new { module = "Playlists" })).WithTags("Playlists");
        return endpoints;
    }
}
