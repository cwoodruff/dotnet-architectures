namespace Chinook.NTier.Business;

/// <summary>What the presentation layer gets back. It never sees the Data layer's row types.</summary>
public sealed record ArtistSummary(int Id, string Name, int AlbumCount);
