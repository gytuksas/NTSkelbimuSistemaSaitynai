using System;
using System.Collections.Generic;

namespace NTSkelbimuSistemaSaitynai.Models;

public partial class Finishtype
{
    public int IdFinishtypes { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Apartment> Apartments { get; set; } = new List<Apartment>();
}
