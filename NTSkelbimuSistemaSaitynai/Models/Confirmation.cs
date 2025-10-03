using System;
using System.Collections.Generic;

namespace NTSkelbimuSistemaSaitynai.Models;

public partial class Confirmation
{
    public string Id { get; set; } = null!;

    public DateOnly Expires { get; set; }

    public long FkBuyeridUser { get; set; }

    public virtual Buyer FkBuyeridUserNavigation { get; set; } = null!;
}
