using Chinook.NTier.Data;

namespace Chinook.NTier.Business;

public sealed class ArtistService(ArtistRepository artists)
{
    public async Task<IReadOnlyList<ArtistSummary>> GetTopTen(CancellationToken cancellationToken = default)
    {
        var rows = await artists.GetTopTen(cancellationToken);
        return rows.Select(r => new ArtistSummary(r.Id, r.Name ?? "(unnamed)", r.AlbumCount)).ToList();
    }
}
