namespace Chinook.EventDriven.Contracts;

/// <summary>Raised by use case 5 when a track is added to a playlist.</summary>
public sealed record TrackAddedToPlaylist(int PlaylistId, int TrackId);
