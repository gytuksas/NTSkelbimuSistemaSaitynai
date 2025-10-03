using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NTSkelbimuSistemaSaitynai.Models;

public partial class Viewingstatus
{
    public int IdViewingstatus { get; set; }

    public string Name { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<Viewing> Viewings { get; set; } = new List<Viewing>();
}
