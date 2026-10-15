namespace Chinook.Bff.Gateway.Bff;

// The gateway's own view of the backend payloads. It deliberately does not reference the
// backend projects: the contract between them is JSON over HTTP, not shared types.
public sealed record ArtistSummary(int Id, string? Name, int AlbumCount);

public sealed record CustomerSummary(int Id, string FirstName, string LastName, string? Country, int? SupportRepId);

public sealed class CatalogClient(HttpClient http)
{
    public async Task<IReadOnlyList<ArtistSummary>> GetTopArtists(CancellationToken cancellationToken) =>
        await http.GetFromJsonAsync<List<ArtistSummary>>("/artists", cancellationToken) ?? [];
}

public sealed class CustomersClient(HttpClient http)
{
    public async Task<IReadOnlyList<CustomerSummary>> GetNewestCustomers(int count, CancellationToken cancellationToken) =>
        await http.GetFromJsonAsync<List<CustomerSummary>>($"/customers/newest?count={count}", cancellationToken) ?? [];
}
