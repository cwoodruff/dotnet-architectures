using System.Net.Http.Json;
using Chinook.Maui.App.Models;

namespace Chinook.Maui.App.Services;

public sealed class ArtistsApiClient(HttpClient http)
{
    public async Task<IReadOnlyList<ArtistItem>> GetTopArtistsAsync(CancellationToken cancellationToken = default) =>
        await http.GetFromJsonAsync<List<ArtistItem>>("artists", cancellationToken) ?? [];
}
