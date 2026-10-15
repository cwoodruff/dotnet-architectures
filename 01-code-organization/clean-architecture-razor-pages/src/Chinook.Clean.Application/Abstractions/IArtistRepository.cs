using Chinook.Clean.Domain;

namespace Chinook.Clean.Application.Abstractions;

/// <summary>
/// Defined here, implemented in Infrastructure. The dependency points inward: Infrastructure
/// knows about Application, never the other way round.
/// </summary>
public interface IArtistRepository
{
    /// <summary>Returns the first <paramref name="count"/> artists by name, with their albums loaded.</summary>
    Task<IReadOnlyList<Artist>> GetFirstByNameWithAlbumsAsync(int count, CancellationToken cancellationToken = default);
}
