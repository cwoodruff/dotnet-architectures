using Dapper;
using Microsoft.Data.SqlClient;

namespace Chinook.EventSourcing.Api.ReadModel;

public sealed record TopArtist(int Id, string Name, int AlbumCount);

/// <summary>
/// The read model is Chinook itself. Queries go straight to its tables with Dapper; the event
/// store is only for writes. Projecting playlist events into Chinook's Playlist and
/// PlaylistTrack tables is the use-case work that closes that loop.
/// </summary>
public sealed class ArtistQueries(string chinookConnectionString)
{
    private const string TopArtistsSql = """
        SELECT TOP (@Count) a.Id, COALESCE(a.Name, N'(unnamed)') AS Name, COUNT(al.Id) AS AlbumCount
        FROM dbo.Artist AS a
        LEFT JOIN dbo.Album AS al ON al.ArtistId = a.Id
        GROUP BY a.Id, a.Name
        ORDER BY a.Name;
        """;

    public async Task<IReadOnlyList<TopArtist>> GetTopArtistsAsync(int count, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(chinookConnectionString);
        var rows = await connection.QueryAsync<TopArtist>(
            new CommandDefinition(TopArtistsSql, new { Count = count }, cancellationToken: cancellationToken));
        return rows.AsList();
    }
}
