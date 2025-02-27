using System;
using System.Collections.Generic;

namespace Reception.Models;

public partial class Session
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Code { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public virtual Account User { get; set; } = null!;
}
