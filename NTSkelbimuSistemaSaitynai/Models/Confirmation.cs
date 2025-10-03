using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NTSkelbimuSistemaSaitynai.Models;

public partial class Confirmation
{
    public string Id { get; set; } = null!;

    public DateTime Expires { get; set; }

    public long FkBuyeridUser { get; set; }

    [JsonIgnore]
    public virtual Buyer FkBuyeridUserNavigation { get; set; } = null!;
}
