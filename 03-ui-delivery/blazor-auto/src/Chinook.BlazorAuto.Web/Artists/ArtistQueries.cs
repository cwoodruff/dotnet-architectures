using Chinook.BlazorAuto.Web.Client.Artists;
using Chinook.BlazorAuto.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace Chinook.BlazorAuto.Web.Artists;

public static class ArtistQueries
{
    public static async Task<IReadOnlyList<ArtistSummary>> GetTopArtistsAsync(
        ChinookDbContext db, CancellationToken cancellationToken) =>
        await db.Artists
            .AsNoTracking()
            .OrderBy(a => a.Name)
            .Take(10)
            .Select(a => new ArtistSummary(a.Id, a.Name ?? "(unnamed)", a.Albums.Count))
            .ToListAsync(cancellationToken);

    /// <summary>GET /api/artists: what the component calls once it is running on WebAssembly.</summary>
    public static IEndpointRouteBuilder MapArtistsApi(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/artists", async (ChinookDbContext db, CancellationToken cancellationToken) =>
            await GetTopArtistsAsync(db, cancellationToken));
        return endpoints;
    }
}
