namespace Chinook.EventDriven.Contracts;

/// <summary>Raised by use case 5 when a track is removed from a playlist.</summary>
public sealed record TrackRemovedFromPlaylist(int PlaylistId, int TrackId);
