using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NTSkelbimuSistemaSaitynai.Models;

public partial class Viewing
{
    public DateTime From { get; set; }

    public DateTime To { get; set; }

    public int Status { get; set; }

    public long IdViewing { get; set; }

    public long FkAvailabilityidAvailability { get; set; }

    [JsonIgnore]
    public virtual Availability FkAvailabilityidAvailabilityNavigation { get; set; } = null!;

    [JsonIgnore]
    public virtual Listing? Listing { get; set; }

    [JsonIgnore]
    public virtual Viewingstatus StatusNavigation { get; set; } = null!;
}
