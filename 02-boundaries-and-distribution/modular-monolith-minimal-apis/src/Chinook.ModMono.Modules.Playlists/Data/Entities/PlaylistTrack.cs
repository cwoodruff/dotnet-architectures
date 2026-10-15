using System;
using System.Collections.Generic;

namespace Chinook.ModMono.Modules.Playlists.Data.Entities;

public partial class PlaylistTrack
{
    public int PlaylistId { get; set; }

    public int TrackId { get; set; }

    public virtual Playlist Playlist { get; set; } = null!;
}
