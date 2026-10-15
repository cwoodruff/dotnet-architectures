using Chinook.ModMono.Modules.Catalog.Data;
using Microsoft.EntityFrameworkCore;

namespace Chinook.ModMono.Modules.Catalog.Features;

public sealed record ArtistSummary(int Id, string? Name, int AlbumCount);

internal static class GetTopArtists
{
    public static async Task<List<ArtistSummary>> Handle(CatalogDbContext db, CancellationToken cancellationToken) =>
        await db.Artists
            .AsNoTracking()
            .OrderBy(a => a.Name)
            .Take(10)
            .Select(a => new ArtistSummary(a.Id, a.Name, a.Albums.Count))
            .ToListAsync(cancellationToken);
}
