using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NTSkelbimuSistemaSaitynai.Models;

public partial class Buyer
{
    public bool Confirmed { get; set; }

    public bool Blocked { get; set; }

    public long IdUser { get; set; }

    [JsonIgnore]
    public virtual Confirmation? Confirmation { get; set; }

    [JsonIgnore]
    public virtual User IdUserNavigation { get; set; } = null!;
}
