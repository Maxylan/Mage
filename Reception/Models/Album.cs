using System;
using System.Collections.Generic;

namespace Reception.Models;

public partial class Album
{
    public int Id { get; set; }

    public int? CategoryId { get; set; }

    public int? ThumbnailId { get; set; }

    public string Title { get; set; } = null!;

    public string? Summary { get; set; }

    public string? Description { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Category? Category { get; set; }

    public virtual Account? CreatedByNavigation { get; set; }

    public virtual Photo? Thumbnail { get; set; }

    public virtual ICollection<Photo> Photos { get; set; } = new List<Photo>();

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
