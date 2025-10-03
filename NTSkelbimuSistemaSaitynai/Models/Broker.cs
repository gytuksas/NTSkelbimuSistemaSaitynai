using System;
using System.Collections.Generic;

namespace NTSkelbimuSistemaSaitynai.Models;

public partial class Broker
{
    public bool Confirmed { get; set; }

    public bool Blocked { get; set; }

    public long IdUser { get; set; }

    public virtual ICollection<Availability> Availabilities { get; set; } = new List<Availability>();

    public virtual ICollection<Building> Buildings { get; set; } = new List<Building>();

    public virtual User IdUserNavigation { get; set; } = null!;
}
