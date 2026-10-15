using Dapper;
using Microsoft.Data.SqlClient;

namespace Chinook.NTier.Data;

public sealed class ArtistRepository(string connectionString)
{
    // The Chinook stored procedures cover single-table reads and one-hop relationships
    // (sproc_GetArtist, sproc_GetAlbumByArtist, ...). None of them aggregates, so the album
    // count needs inline SQL. This is the first place a sproc-only data layer leaks.
    private const string TopTenSql = """
        SELECT TOP (10) a.Id, a.Name, COUNT(al.Id) AS AlbumCount
        FROM dbo.Artist AS a
        LEFT JOIN dbo.Album AS al ON al.ArtistId = a.Id
        GROUP BY a.Id, a.Name
        ORDER BY a.Name;
        """;

    public async Task<IReadOnlyList<ArtistRow>> GetTopTen(CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(connectionString);
        var rows = await connection.QueryAsync<ArtistRow>(
            new CommandDefinition(TopTenSql, cancellationToken: cancellationToken));
        return rows.AsList();
    }
}
