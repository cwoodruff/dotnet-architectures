using Chinook.VerticalSlice.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Chinook.VerticalSlice.Api.Features.Artists;

/// <summary>
/// One slice, one file: the request, the response, the handler, and the endpoint mapping for
/// "the first ten artists by name, with their album counts". Nothing outside this file is
/// specific to the feature.
/// </summary>
public static class GetTopArtists
{
    public sealed record Request(int Count = 10);

    public sealed record Response(int Id, string Name, int AlbumCount);

    public sealed class Handler(ChinookDbContext db)
    {
        public async Task<IReadOnlyList<Response>> HandleAsync(Request request, CancellationToken cancellationToken) =>
            await db.Artists
                .AsNoTracking()
                .OrderBy(a => a.Name)
                .Take(request.Count)
                .Select(a => new Response(a.Id, a.Name ?? "(unnamed)", a.Albums.Count))
                .ToListAsync(cancellationToken);
    }

    public static IServiceCollection AddGetTopArtists(this IServiceCollection services) =>
        services.AddScoped<Handler>();

    public static IEndpointRouteBuilder MapGetTopArtists(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/artists", async (Handler handler, CancellationToken cancellationToken) =>
                TypedResults.Ok(await handler.HandleAsync(new Request(), cancellationToken)))
            .WithName("GetTopArtists")
            .WithTags("Artists");

        return endpoints;
    }
}
