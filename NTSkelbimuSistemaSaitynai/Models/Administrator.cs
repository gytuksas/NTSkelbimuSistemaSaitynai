using System;
using System.Collections.Generic;

namespace NTSkelbimuSistemaSaitynai.Models;

public partial class Administrator
{
    public long IdUser { get; set; }

    public virtual User IdUserNavigation { get; set; } = null!;
}
