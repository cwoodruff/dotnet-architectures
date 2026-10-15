using Chinook.EventSourcing.Api.Domain;
using Chinook.EventSourcing.Api.EventStore;

namespace Chinook.EventSourcing.Api.Endpoints;

public static class PlaylistEndpoints
{
    public sealed record CreatePlaylistRequest(string? Name);

    public sealed record CreatePlaylistResponse(Guid StreamId);

    public static IEndpointRouteBuilder MapPlaylistEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/playlists", async (CreatePlaylistRequest request, SqlEventStore store, CancellationToken cancellationToken) =>
            {
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    return Results.ValidationProblem(new Dictionary<string, string[]>
                    {
                        ["name"] = ["A playlist name is required."],
                    });
                }

                var playlist = Playlist.Create(Guid.NewGuid(), request.Name);
                await store.AppendAsync(playlist.Id, playlist.Version, playlist.UncommittedEvents, cancellationToken);
                playlist.MarkCommitted();

                // Nothing is written to Chinook's Playlist table yet; that is the projection's job.
                return Results.Created($"/playlists/{playlist.Id}", new CreatePlaylistResponse(playlist.Id));
            })
            .WithName("CreatePlaylist")
            .WithTags("Playlists");

        return endpoints;
    }
}
