namespace Chinook.Strangler.Legacy.Models;

public class ArtistRecord
{
    public int Id { get; set; }
    public string? Name { get; set; }
}

public class AlbumRecord
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public int ArtistId { get; set; }
}

public class AlbumsViewModel
{
    public int ArtistId { get; set; }
    public List<AlbumRecord> Albums { get; set; } = new();
}
