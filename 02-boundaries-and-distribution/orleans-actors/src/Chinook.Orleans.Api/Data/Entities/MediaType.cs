using System;
using System.Collections.Generic;

namespace Chinook.Orleans.Api.Data.Entities;

public partial class MediaType
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Track> Tracks { get; set; } = new List<Track>();
}
