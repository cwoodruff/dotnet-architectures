using Chinook.BlazorAuto.Web.Client.Artists;
using Chinook.BlazorAuto.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace Chinook.BlazorAuto.Web.Artists;

/// <summary>
/// The server-side implementation, used while the component prerenders and while it runs over a circuit.
/// It runs the same query as GET /api/artists directly, rather than calling the app's own endpoint over HTTP.
/// A factory gives each call its own short-lived context, because a circuit outlives a request scope.
/// </summary>
public sealed class ServerArtistsClient(IDbContextFactory<ChinookDbContext> dbFactory) : IArtistsClient
{
    public async Task<IReadOnlyList<ArtistSummary>> GetTopArtistsAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        return await ArtistQueries.GetTopArtistsAsync(db, cancellationToken);
    }
}
