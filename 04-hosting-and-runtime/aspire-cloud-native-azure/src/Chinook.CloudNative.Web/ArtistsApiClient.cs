namespace Chinook.CloudNative.Web;

/// <summary>Typed client for the ApiService. The base address is a service discovery name, not a URL.</summary>
public class ArtistsApiClient(HttpClient httpClient)
{
    public async Task<ArtistSummary[]> GetTopArtistsAsync(CancellationToken cancellationToken = default) =>
        await httpClient.GetFromJsonAsync<ArtistSummary[]>("/artists", cancellationToken) ?? [];
}

public record ArtistSummary(int Id, string? Name, int AlbumCount);
