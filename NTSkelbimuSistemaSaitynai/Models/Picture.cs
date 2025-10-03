using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NTSkelbimuSistemaSaitynai.Models;

public partial class Picture
{
    public string Id { get; set; } = null!;

    public bool Public { get; set; }

    public long FkApartmentidApartment { get; set; }

    [JsonIgnore]
    public virtual Apartment FkApartmentidApartmentNavigation { get; set; } = null!;

    [JsonIgnore]
    public virtual Listing? Listing { get; set; }
}
