using Chinook.Orleans.Grains.Abstractions;
using Orleans.Runtime;

namespace Chinook.Orleans.Grains;

public sealed class ArtistGrain(
    [PersistentState("artist", GrainStorage.Name)] IPersistentState<ArtistGrainState> state) : Grain, IArtistGrain
{
    public async Task<int> RecordView()
    {
        state.State.Views++;
        await state.WriteStateAsync();
        return state.State.Views;
    }
}

[GenerateSerializer]
public sealed class ArtistGrainState
{
    [Id(0)]
    public int Views { get; set; }
}
