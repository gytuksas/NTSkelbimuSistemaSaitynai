using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NTSkelbimuSistemaSaitynai.Models;

public partial class Energyclass
{
    public int IdEnergyclass { get; set; }

    public string Name { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<Building> Buildings { get; set; } = new List<Building>();
}
