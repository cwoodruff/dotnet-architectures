using Chinook.Cqrs.Api.Data;
using Chinook.Cqrs.Api.Data.Entities;
using Chinook.Cqrs.Api.Dispatching;

namespace Chinook.Cqrs.Api.Commands;

/// <summary>Creates an empty playlist and returns its new Id.</summary>
public sealed record CreatePlaylistCommand(string Name) : ICommand<int>;

/// <summary>Write side: goes through the EF Core model so the change tracker owns the insert.</summary>
public sealed class CreatePlaylistCommandHandler(ChinookDbContext db) : ICommandHandler<CreatePlaylistCommand, int>
{
    public async Task<int> HandleAsync(CreatePlaylistCommand command, CancellationToken cancellationToken)
    {
        var playlist = new Playlist { Name = command.Name.Trim() };
        db.Playlists.Add(playlist);
        await db.SaveChangesAsync(cancellationToken);
        return playlist.Id;
    }
}
