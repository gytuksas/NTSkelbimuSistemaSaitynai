using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NTSkelbimuSistemaSaitynai.Models;

public partial class Finishtype
{
    public int IdFinishtypes { get; set; }

    public string Name { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<Apartment> Apartments { get; set; } = new List<Apartment>();
}
