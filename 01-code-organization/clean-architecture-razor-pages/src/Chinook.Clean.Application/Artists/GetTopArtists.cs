using Chinook.Clean.Application.Abstractions;

namespace Chinook.Clean.Application.Artists;

public sealed record TopArtist(int Id, string Name, int AlbumCount);

/// <summary>Use case: the first ten artists by name, each with the number of albums they have.</summary>
public sealed class GetTopArtists(IArtistRepository artists)
{
    public const int Count = 10;

    public async Task<IReadOnlyList<TopArtist>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var result = await artists.GetFirstByNameWithAlbumsAsync(Count, cancellationToken);
        return result.Select(a => new TopArtist(a.Id, a.Name ?? "(unnamed)", a.Albums.Count)).ToList();
    }
}
