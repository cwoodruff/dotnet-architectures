using Chinook.Htmx.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Chinook.Htmx.Web.Pages;

public class ArtistsModel(ChinookDbContext db) : PageModel
{
    public IReadOnlyList<ArtistSummary> Artists { get; private set; } = [];

    // Full page: the first request renders the layout and the list together.
    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Artists = await LoadTopTenAsync(cancellationToken);
    }

    // htmx target: hx-get="/Artists?handler=List" asks for just the partial, which htmx swaps into the page.
    public async Task<PartialViewResult> OnGetListAsync(CancellationToken cancellationToken)
    {
        Artists = await LoadTopTenAsync(cancellationToken);
        return Partial("_ArtistList", Artists);
    }

    private async Task<IReadOnlyList<ArtistSummary>> LoadTopTenAsync(CancellationToken cancellationToken) =>
        await db.Artists
            .AsNoTracking()
            .OrderBy(a => a.Name)
            .Take(10)
            .Select(a => new ArtistSummary(a.Id, a.Name ?? "(unnamed)", a.Albums.Count))
            .ToListAsync(cancellationToken);
}
