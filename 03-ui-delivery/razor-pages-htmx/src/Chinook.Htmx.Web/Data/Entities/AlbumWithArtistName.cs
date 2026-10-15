using System;
using System.Collections.Generic;

namespace Chinook.Htmx.Web.Data.Entities;

public partial class AlbumWithArtistName
{
    public int AlbumId { get; set; }

    public string Title { get; set; } = null!;

    public int ArtistId { get; set; }

    public string? Name { get; set; }
}
