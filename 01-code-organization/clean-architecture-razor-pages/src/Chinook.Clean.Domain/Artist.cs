namespace Chinook.Clean.Domain;

/// <summary>
/// Hand-written domain entity for the Chinook Artist table. Nothing here knows about EF Core;
/// the mapping lives in Infrastructure.
/// </summary>
public sealed class Artist
{
    private readonly List<Album> _albums = [];

    // For the persistence layer, which materializes entities without going through the public constructor.
    private Artist()
    {
    }

    public Artist(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }

    public int Id { get; private set; }

    public string? Name { get; private set; }

    public IReadOnlyCollection<Album> Albums => _albums;
}
