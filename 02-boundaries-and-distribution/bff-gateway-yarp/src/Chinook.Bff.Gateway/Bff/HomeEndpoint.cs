namespace Chinook.Bff.Gateway.Bff;

/// <summary>
/// The BFF part of the gateway. The proxy routes forward one request to one backend; this
/// endpoint calls both backends in parallel and returns one payload shaped for the home screen,
/// so the client makes one round trip instead of two.
/// </summary>
public static class HomeEndpoint
{
    public sealed record HomeView(IReadOnlyList<ArtistSummary> TopArtists, IReadOnlyList<CustomerSummary> NewestCustomers);

    public static IEndpointRouteBuilder MapBffHome(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/bff/home", async (CatalogClient catalog, CustomersClient customers, CancellationToken cancellationToken) =>
        {
            var artists = catalog.GetTopArtists(cancellationToken);
            var newest = customers.GetNewestCustomers(5, cancellationToken);
            await Task.WhenAll(artists, newest);
            return new HomeView(await artists, await newest);
        });
        return endpoints;
    }
}
