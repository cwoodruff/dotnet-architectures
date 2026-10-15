using System.Net.Http.Json;

namespace Chinook.BlazorAuto.Web.Client.Artists;

/// <summary>The browser-side implementation: WebAssembly cannot reach SQL Server, so it calls GET /api/artists.</summary>
public sealed class HttpArtistsClient(HttpClient http) : IArtistsClient
{
    public async Task<IReadOnlyList<ArtistSummary>> GetTopArtistsAsync(CancellationToken cancellationToken = default) =>
        await http.GetFromJsonAsync<List<ArtistSummary>>("api/artists", cancellationToken) ?? [];
}
