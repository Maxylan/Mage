using System;
using System.Collections.Generic;

namespace Reception.Models;

public partial class Category
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Summary { get; set; }

    public string? Description { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Album> Albums { get; set; } = new List<Album>();

    public virtual Account? CreatedByNavigation { get; set; }
}
