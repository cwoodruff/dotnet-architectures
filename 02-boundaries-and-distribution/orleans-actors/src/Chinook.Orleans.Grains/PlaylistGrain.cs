using Chinook.Orleans.Grains.Abstractions;
using Orleans.Runtime;

namespace Chinook.Orleans.Grains;

public sealed class PlaylistGrain(
    [PersistentState("playlist", GrainStorage.Name)] IPersistentState<PlaylistGrainState> state,
    ILocalSiloDetails silo) : Grain, IPlaylistGrain
{
    public async Task<PingResponse> Ping()
    {
        state.State.PingCount++;
        await state.WriteStateAsync();
        return new PingResponse(this.GetPrimaryKeyLong(), state.State.PingCount, silo.SiloAddress.ToString());
    }
}

[GenerateSerializer]
public sealed class PlaylistGrainState
{
    [Id(0)]
    public int PingCount { get; set; }
}
