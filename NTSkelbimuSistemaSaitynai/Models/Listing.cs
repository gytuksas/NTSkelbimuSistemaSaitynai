using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NTSkelbimuSistemaSaitynai.Models;

public partial class Listing
{
    public string Description { get; set; } = null!;

    public double Askingprice { get; set; }

    public bool Rent { get; set; }

    public long IdListing { get; set; }

    public long FkViewingidViewing { get; set; }

    public string FkPictureid { get; set; } = null!;

    [JsonIgnore]
    public virtual Picture FkPicture { get; set; } = null!;

    [JsonIgnore]
    public virtual Viewing FkViewingidViewingNavigation { get; set; } = null!;
}
