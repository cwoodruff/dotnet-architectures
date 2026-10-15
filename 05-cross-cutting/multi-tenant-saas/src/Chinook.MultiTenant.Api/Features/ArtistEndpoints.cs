using Chinook.MultiTenant.Api.Data;
using Chinook.MultiTenant.Api.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace Chinook.MultiTenant.Api.Features;

public sealed record ArtistSummary(int Id, string? Name, int AlbumCount);

public sealed record TopArtistsResponse(string Tenant, IReadOnlyList<ArtistSummary> Artists);

public static class ArtistEndpoints
{
    public static IEndpointRouteBuilder MapArtistEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/artists", async (ChinookDbContext db, ITenantContext tenant, CancellationToken ct) =>
        {
            var artists = await db.Artists
                .OrderBy(a => a.Name)
                .Take(10)
                .Select(a => new ArtistSummary(a.Id, a.Name, a.Albums.Count))
                .ToListAsync(ct);

            return new TopArtistsResponse(tenant.Current!.Id, artists);
        });

        return app;
    }
}
