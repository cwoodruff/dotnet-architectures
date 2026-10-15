using Chinook.Clean.Application.Artists;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chinook.Clean.Web.Pages;

public class ArtistsModel(GetTopArtists getTopArtists) : PageModel
{
    public IReadOnlyList<TopArtist> Artists { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Artists = await getTopArtists.ExecuteAsync(cancellationToken);
    }
}
