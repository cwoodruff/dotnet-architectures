using Chinook.NTier.Business;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chinook.NTier.Web.Pages;

public class ArtistsModel(ArtistService artistService) : PageModel
{
    public IReadOnlyList<ArtistSummary> Artists { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Artists = await artistService.GetTopTen(cancellationToken);
    }
}
