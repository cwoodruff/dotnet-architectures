using Chinook.Clean.Application.Abstractions;
using Chinook.Clean.Domain;
using Chinook.Clean.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Chinook.Clean.Infrastructure.Repositories;

internal sealed class ArtistRepository(ChinookDbContext db) : IArtistRepository
{
    public async Task<IReadOnlyList<Artist>> GetFirstByNameWithAlbumsAsync(int count, CancellationToken cancellationToken = default) =>
        await db.Artists
            .AsNoTracking()
            .Include(a => a.Albums)
            .OrderBy(a => a.Name)
            .Take(count)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);
}
