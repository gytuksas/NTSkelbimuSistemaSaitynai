using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NTSkelbimuSistemaSaitynai.Models;

public partial class Administrator
{
    public long IdUser { get; set; }

    [JsonIgnore]
    public virtual User IdUserNavigation { get; set; } = null!;
}
