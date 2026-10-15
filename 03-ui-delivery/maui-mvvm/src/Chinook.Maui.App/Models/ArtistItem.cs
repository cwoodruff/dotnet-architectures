namespace Chinook.Maui.App.Models;

/// <summary>One row in the artists list, shaped like the JSON the Api returns.</summary>
public sealed record ArtistItem(int Id, string Name, int AlbumCount);
