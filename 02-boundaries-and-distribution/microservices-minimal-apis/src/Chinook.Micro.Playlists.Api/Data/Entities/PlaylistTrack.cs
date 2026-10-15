using System;
using System.Collections.Generic;

namespace Chinook.Micro.Playlists.Api.Data.Entities;

public partial class PlaylistTrack
{
    public int PlaylistId { get; set; }

    public int TrackId { get; set; }

    public virtual Playlist Playlist { get; set; } = null!;
}
