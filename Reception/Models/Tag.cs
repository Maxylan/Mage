using System;
using System.Collections.Generic;

namespace Reception.Models;

public partial class Tag
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Album> Albums { get; set; } = new List<Album>();

    public virtual ICollection<Photo> Photos { get; set; } = new List<Photo>();
}
