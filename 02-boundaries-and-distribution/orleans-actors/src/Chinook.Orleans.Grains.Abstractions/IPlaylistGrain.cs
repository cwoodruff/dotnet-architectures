namespace Chinook.Orleans.Grains.Abstractions;

/// <summary>
/// One activation per Chinook playlist id. Today it only answers a ping, which proves the
/// client-to-silo round trip and the grain storage. Use case 5 (add or remove tracks, reject
/// duplicates) will move here, with the playlist's track list as the grain's state.
/// </summary>
public interface IPlaylistGrain : IGrainWithIntegerKey
{
    Task<PingResponse> Ping();
}

[GenerateSerializer, Immutable]
public sealed record PingResponse(
    [property: Id(0)] long PlaylistId,
    [property: Id(1)] int PingCount,
    [property: Id(2)] string SiloAddress);
