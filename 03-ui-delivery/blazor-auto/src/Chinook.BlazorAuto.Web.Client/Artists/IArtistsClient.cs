namespace Chinook.BlazorAuto.Web.Client.Artists;

/// <summary>
/// How the Artists page gets its data. An InteractiveAuto component can run in two places: on the server
/// (while prerendering, and over a SignalR circuit before the WebAssembly runtime has downloaded) and in the
/// browser (once it has). Each host registers its own implementation.
/// </summary>
public interface IArtistsClient
{
    Task<IReadOnlyList<ArtistSummary>> GetTopArtistsAsync(CancellationToken cancellationToken = default);
}
