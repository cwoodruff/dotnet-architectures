using Chinook.EventSourcing.Api.ReadModel;

namespace Chinook.EventSourcing.Api.Endpoints;

public static class ArtistEndpoints
{
    public static IEndpointRouteBuilder MapArtistEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/artists", async (ArtistQueries queries, CancellationToken cancellationToken) =>
                TypedResults.Ok(await queries.GetTopArtistsAsync(10, cancellationToken)))
            .WithName("GetTopArtists")
            .WithTags("Artists");

        return endpoints;
    }
}
