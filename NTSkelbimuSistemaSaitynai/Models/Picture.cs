using System;
using System.Collections.Generic;

namespace NTSkelbimuSistemaSaitynai.Models;

public partial class Picture
{
    public string Id { get; set; } = null!;

    public bool Public { get; set; }

    public long FkApartmentidApartment { get; set; }

    public virtual Apartment FkApartmentidApartmentNavigation { get; set; } = null!;

    public virtual Listing? Listing { get; set; }
}
