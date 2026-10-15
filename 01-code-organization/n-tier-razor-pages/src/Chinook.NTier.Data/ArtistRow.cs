namespace Chinook.NTier.Data;

/// <summary>One row of the top-artists query, shaped like the SQL result.</summary>
public sealed record ArtistRow(int Id, string? Name, int AlbumCount);
