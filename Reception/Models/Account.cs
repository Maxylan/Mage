using System;
using System.Collections.Generic;

namespace Reception.Models;

public partial class Account
{
    public int Id { get; set; }

    public string? Email { get; set; }

    public string? Username { get; set; }

    public string Password { get; set; } = null!;

    public string? FullName { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime LastVisit { get; set; }

    public short Permissions { get; set; }

    public virtual ICollection<Album> Albums { get; set; } = new List<Album>();

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    public virtual ICollection<Photo> Photos { get; set; } = new List<Photo>();

    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
}
