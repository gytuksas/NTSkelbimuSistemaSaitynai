using System;
using System.Collections.Generic;

namespace NTSkelbimuSistemaSaitynai.Models;

public partial class Energyclass
{
    public int IdEnergyclass { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Building> Buildings { get; set; } = new List<Building>();
}
