using System;
using System.Collections.Generic;

namespace Chinook.Micro.Playlists.Api.Data.Entities;

public partial class Playlist
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<PlaylistTrack> PlaylistTracks { get; set; } = new List<PlaylistTrack>();
}
