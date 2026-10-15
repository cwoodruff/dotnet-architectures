using Chinook.Cqrs.Api.Commands;
using Chinook.Cqrs.Api.Dispatching;

namespace Chinook.Cqrs.Api.Endpoints;

public static class PlaylistEndpoints
{
    public sealed record CreatePlaylistRequest(string? Name);

    public sealed record CreatePlaylistResponse(int Id);

    public static IEndpointRouteBuilder MapPlaylistEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/playlists", async (CreatePlaylistRequest request, Dispatcher dispatcher, CancellationToken cancellationToken) =>
            {
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    return Results.ValidationProblem(new Dictionary<string, string[]>
                    {
                        ["name"] = ["A playlist name is required."],
                    });
                }

                var id = await dispatcher.SendAsync(new CreatePlaylistCommand(request.Name), cancellationToken);
                return Results.Created($"/playlists/{id}", new CreatePlaylistResponse(id));
            })
            .WithName("CreatePlaylist")
            .WithTags("Playlists");

        return endpoints;
    }
}
