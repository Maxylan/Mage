using System;
using System.Collections.Generic;

namespace Reception.Models;

public partial class Log
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public string? UserEmail { get; set; }

    public string? UserUsername { get; set; }

    public string? UserFullName { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? Action { get; set; }

    public string? Log1 { get; set; }
}
