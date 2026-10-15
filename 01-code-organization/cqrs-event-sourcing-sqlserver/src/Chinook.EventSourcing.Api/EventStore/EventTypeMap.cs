using Chinook.EventSourcing.Api.Domain;

namespace Chinook.EventSourcing.Api.EventStore;

/// <summary>
/// Maps the EventType column to CLR types and back. Stored names are explicit strings, not
/// type names, so renaming a C# record does not orphan the events already in the table.
/// </summary>
public sealed class EventTypeMap
{
    private readonly Dictionary<string, Type> _byName = new(StringComparer.Ordinal);
    private readonly Dictionary<Type, string> _byType = [];

    public EventTypeMap()
    {
        Register<PlaylistCreated>("PlaylistCreated");
        Register<TrackAddedToPlaylist>("TrackAddedToPlaylist");
        Register<TrackRemovedFromPlaylist>("TrackRemovedFromPlaylist");
    }

    public string NameOf(Type type) =>
        _byType.TryGetValue(type, out var name) ? name : throw new InvalidOperationException($"{type.Name} is not a registered event.");

    public Type TypeOf(string name) =>
        _byName.TryGetValue(name, out var type) ? type : throw new InvalidOperationException($"Unknown event type '{name}'.");

    private void Register<T>(string name)
    {
        _byName.Add(name, typeof(T));
        _byType.Add(typeof(T), name);
    }
}
