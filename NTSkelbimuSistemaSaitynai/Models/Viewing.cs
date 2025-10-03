using System;
using System.Collections.Generic;

namespace NTSkelbimuSistemaSaitynai.Models;

public partial class Viewing
{
    public DateOnly From { get; set; }

    public DateOnly To { get; set; }

    public int Status { get; set; }

    public long IdViewing { get; set; }

    public long FkAvailabilityidAvailability { get; set; }

    public virtual Availability FkAvailabilityidAvailabilityNavigation { get; set; } = null!;

    public virtual Listing? Listing { get; set; }

    public virtual Viewingstatus StatusNavigation { get; set; } = null!;
}
