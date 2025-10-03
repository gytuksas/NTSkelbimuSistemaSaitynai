using System;
using System.Collections.Generic;

namespace NTSkelbimuSistemaSaitynai.Models;

public partial class Listing
{
    public string Description { get; set; } = null!;

    public double Askingprice { get; set; }

    public bool Rent { get; set; }

    public long IdListing { get; set; }

    public long FkViewingidViewing { get; set; }

    public string FkPictureid { get; set; } = null!;

    public virtual Picture FkPicture { get; set; } = null!;

    public virtual Viewing FkViewingidViewingNavigation { get; set; } = null!;
}
