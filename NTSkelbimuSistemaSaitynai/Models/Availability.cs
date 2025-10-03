using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NTSkelbimuSistemaSaitynai.Models;

public partial class Availability
{
    public DateTime From { get; set; }

    public DateTime To { get; set; }

    public long IdAvailability { get; set; }

    public long FkBrokeridUser { get; set; }

    [JsonIgnore]
    public virtual Broker FkBrokeridUserNavigation { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<Viewing> Viewings { get; set; } = new List<Viewing>();
}
