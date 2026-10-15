using System;
using System.Collections.Generic;

namespace Chinook.BlazorWasm.Api.Data.Entities;

public partial class Artist
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Album> Albums { get; set; } = new List<Album>();
}
