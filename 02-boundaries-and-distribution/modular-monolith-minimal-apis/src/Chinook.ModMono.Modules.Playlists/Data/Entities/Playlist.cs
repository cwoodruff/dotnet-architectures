using System;
using System.Collections.Generic;

namespace Chinook.ModMono.Modules.Playlists.Data.Entities;

public partial class Playlist
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<PlaylistTrack> PlaylistTracks { get; set; } = new List<PlaylistTrack>();
}
