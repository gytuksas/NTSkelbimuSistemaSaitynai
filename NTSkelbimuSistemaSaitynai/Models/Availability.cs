using System;
using System.Collections.Generic;

namespace NTSkelbimuSistemaSaitynai.Models;

public partial class Availability
{
    public DateOnly From { get; set; }

    public DateOnly To { get; set; }

    public long IdAvailability { get; set; }

    public long FkBrokeridUser { get; set; }

    public virtual Broker FkBrokeridUserNavigation { get; set; } = null!;

    public virtual ICollection<Viewing> Viewings { get; set; } = new List<Viewing>();
}
