using System;
using System.Collections.Generic;

namespace Reception.Models;

public partial class Filepath
{
    public int Id { get; set; }

    public int PhotoId { get; set; }

    public string Filename { get; set; } = null!;

    public int Filesize { get; set; }

    public virtual Photo Photo { get; set; } = null!;
}
