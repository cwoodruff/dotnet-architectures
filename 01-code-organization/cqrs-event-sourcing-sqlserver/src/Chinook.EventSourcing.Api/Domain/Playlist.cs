namespace Chinook.EventSourcing.Api.Domain;

/// <summary>
/// An event-sourced playlist. Its state is never saved; it is rebuilt by replaying its events
/// through the Apply methods. Commands check invariants against that state and raise new events.
/// </summary>
public sealed class Playlist
{
    private readonly HashSet<int> _trackIds = [];
    private readonly List<object> _uncommitted = [];

    private Playlist()
    {
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    /// <summary>The number of events already stored for this stream.</summary>
    public int Version { get; private set; }

    public IReadOnlyCollection<int> TrackIds => _trackIds;

    public IReadOnlyList<object> UncommittedEvents => _uncommitted;

    public static Playlist Create(Guid id, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var playlist = new Playlist();
        playlist.Raise(new PlaylistCreated(id, name.Trim()));
        return playlist;
    }

    public static Playlist FromHistory(IEnumerable<object> history)
    {
        var playlist = new Playlist();
        foreach (var @event in history)
        {
            playlist.ApplyEvent(@event);
            playlist.Version++;
        }

        return playlist;
    }

    public void AddTrack(int trackId)
    {
        // Invariant 2: a track appears at most once on a playlist. The event stream has no
        // composite key to fall back on, so the aggregate is the only thing enforcing it.
        if (_trackIds.Contains(trackId))
        {
            throw new InvalidOperationException($"Track {trackId} is already on playlist {Id}.");
        }

        Raise(new TrackAddedToPlaylist(Id, trackId));
    }

    public void RemoveTrack(int trackId)
    {
        if (!_trackIds.Contains(trackId))
        {
            throw new InvalidOperationException($"Track {trackId} is not on playlist {Id}.");
        }

        Raise(new TrackRemovedFromPlaylist(Id, trackId));
    }

    /// <summary>Called after the store has appended the uncommitted events.</summary>
    public void MarkCommitted()
    {
        Version += _uncommitted.Count;
        _uncommitted.Clear();
    }

    public void Apply(PlaylistCreated @event)
    {
        Id = @event.PlaylistId;
        Name = @event.Name;
    }

    public void Apply(TrackAddedToPlaylist @event) => _trackIds.Add(@event.TrackId);

    public void Apply(TrackRemovedFromPlaylist @event) => _trackIds.Remove(@event.TrackId);

    private void Raise(object @event)
    {
        ApplyEvent(@event);
        _uncommitted.Add(@event);
    }

    private void ApplyEvent(object @event)
    {
        switch (@event)
        {
            case PlaylistCreated e: Apply(e); break;
            case TrackAddedToPlaylist e: Apply(e); break;
            case TrackRemovedFromPlaylist e: Apply(e); break;
            default: throw new InvalidOperationException($"Playlist cannot apply {@event.GetType().Name}.");
        }
    }
}
