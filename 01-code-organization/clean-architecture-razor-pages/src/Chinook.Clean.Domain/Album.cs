namespace Chinook.Clean.Domain;

/// <summary>Hand-written domain entity for the Chinook Album table.</summary>
public sealed class Album
{
    private Album()
    {
        Title = string.Empty;
    }

    public Album(string title, int artistId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        Title = title;
        ArtistId = artistId;
    }

    public int Id { get; private set; }

    public string Title { get; private set; }

    public int ArtistId { get; private set; }
}
