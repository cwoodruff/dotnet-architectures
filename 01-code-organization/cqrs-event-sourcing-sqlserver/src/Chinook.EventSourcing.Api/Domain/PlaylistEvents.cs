namespace Chinook.EventSourcing.Api.Domain;

// The stream id is the playlist's identity, so the events carry it as a Guid rather than
// Chinook's int Playlist.Id. Mapping a stream to a Chinook Playlist row is projection work.

/// <summary>Starts a playlist stream. Internal to this example; not one of the shared domain events.</summary>
public sealed record PlaylistCreated(Guid PlaylistId, string Name);

public sealed record TrackAddedToPlaylist(Guid PlaylistId, int TrackId);

public sealed record TrackRemovedFromPlaylist(Guid PlaylistId, int TrackId);
