namespace Chinook.Orleans.Grains.Abstractions;

/// <summary>
/// One activation per Chinook artist id. A placeholder for catalog behavior that benefits from
/// living in memory next to its state, such as a per-artist view counter.
/// </summary>
public interface IArtistGrain : IGrainWithIntegerKey
{
    Task<int> RecordView();
}
