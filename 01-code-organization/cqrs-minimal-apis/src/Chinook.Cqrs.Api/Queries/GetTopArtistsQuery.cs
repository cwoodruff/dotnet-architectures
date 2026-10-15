using Chinook.Cqrs.Api.Dispatching;
using Dapper;

namespace Chinook.Cqrs.Api.Queries;

public sealed record GetTopArtistsQuery(int Count = 10) : IQuery<IReadOnlyList<TopArtist>>;

public sealed record TopArtist(int Id, string Name, int AlbumCount);

/// <summary>Read side: hand-written SQL through Dapper, shaped exactly like the response.</summary>
public sealed class GetTopArtistsQueryHandler(SqlConnectionFactory connections)
    : IQueryHandler<GetTopArtistsQuery, IReadOnlyList<TopArtist>>
{
    private const string Sql = """
        SELECT TOP (@Count) a.Id, COALESCE(a.Name, N'(unnamed)') AS Name, COUNT(al.Id) AS AlbumCount
        FROM dbo.Artist AS a
        LEFT JOIN dbo.Album AS al ON al.ArtistId = a.Id
        GROUP BY a.Id, a.Name
        ORDER BY a.Name;
        """;

    public async Task<IReadOnlyList<TopArtist>> HandleAsync(GetTopArtistsQuery query, CancellationToken cancellationToken)
    {
        await using var connection = connections.Create();
        var rows = await connection.QueryAsync<TopArtist>(
            new CommandDefinition(Sql, new { query.Count }, cancellationToken: cancellationToken));
        return rows.AsList();
    }
}
