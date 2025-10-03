using System;
using System.Collections.Generic;

namespace NTSkelbimuSistemaSaitynai.Models;

public partial class Viewingstatus
{
    public int IdViewingstatus { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Viewing> Viewings { get; set; } = new List<Viewing>();
}
