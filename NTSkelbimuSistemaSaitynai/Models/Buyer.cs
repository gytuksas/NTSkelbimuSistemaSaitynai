using System;
using System.Collections.Generic;

namespace NTSkelbimuSistemaSaitynai.Models;

public partial class Buyer
{
    public bool Confirmed { get; set; }

    public bool Blocked { get; set; }

    public long IdUser { get; set; }

    public virtual Confirmation? Confirmation { get; set; }

    public virtual User IdUserNavigation { get; set; } = null!;
}
