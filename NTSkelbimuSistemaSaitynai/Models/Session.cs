using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NTSkelbimuSistemaSaitynai.Models;

public partial class Session
{
    public string Id { get; set; } = null!;

    public DateTime Created { get; set; }

    public bool Remember { get; set; }

    public DateTime Lastactivity { get; set; }

    public long FkUseridUser { get; set; }

    [JsonIgnore]
    public virtual User FkUseridUserNavigation { get; set; } = null!;
}
