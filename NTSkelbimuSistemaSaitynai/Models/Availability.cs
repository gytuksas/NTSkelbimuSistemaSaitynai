using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NTSkelbimuSistemaSaitynai.Models;

public partial class Availability
{
    public DateOnly From { get; set; }

    public DateOnly To { get; set; }

    public long IdAvailability { get; set; }

    public long FkBrokeridUser { get; set; }

    [JsonIgnore]
    public virtual Broker FkBrokeridUserNavigation { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<Viewing> Viewings { get; set; } = new List<Viewing>();
}
