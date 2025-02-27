using System;
using System.Collections.Generic;

namespace Reception.Models;

public partial class Photo
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Title { get; set; }

    public string? Summary { get; set; }

    public string? Description { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Album> Albums { get; set; } = new List<Album>();

    public virtual Account? CreatedByNavigation { get; set; }

    public virtual ICollection<Filepath> Filepaths { get; set; } = new List<Filepath>();

    public virtual ICollection<Album> AlbumsNavigation { get; set; } = new List<Album>();

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
