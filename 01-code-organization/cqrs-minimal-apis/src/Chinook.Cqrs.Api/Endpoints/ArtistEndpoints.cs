using Chinook.Cqrs.Api.Dispatching;
using Chinook.Cqrs.Api.Queries;

namespace Chinook.Cqrs.Api.Endpoints;

public static class ArtistEndpoints
{
    public static IEndpointRouteBuilder MapArtistEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/artists", async (Dispatcher dispatcher, CancellationToken cancellationToken) =>
                TypedResults.Ok(await dispatcher.QueryAsync(new GetTopArtistsQuery(), cancellationToken)))
            .WithName("GetTopArtists")
            .WithTags("Artists");

        return endpoints;
    }
}
