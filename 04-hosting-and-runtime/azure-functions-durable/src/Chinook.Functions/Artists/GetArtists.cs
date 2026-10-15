using Chinook.Functions.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;

namespace Chinook.Functions.Artists;

public sealed record ArtistSummary(int Id, string? Name, int AlbumCount);

/// <summary>GET /api/artists: the first ten artists by name, with their album counts.</summary>
public sealed class GetArtists(ChinookDbContext db)
{
    [Function(nameof(GetArtists))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "artists")] HttpRequest request,
        CancellationToken cancellationToken)
    {
        var artists = await db.Artists
            .OrderBy(a => a.Name)
            .Take(10)
            .Select(a => new ArtistSummary(a.Id, a.Name, a.Albums.Count))
            .ToListAsync(cancellationToken);

        return new OkObjectResult(artists);
    }
}
