using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NTSkelbimuSistemaSaitynai.Models;

public partial class Session
{
    public string Id { get; set; } = null!;

    public DateOnly Created { get; set; }

    public bool Remember { get; set; }

    public DateOnly Lastactivity { get; set; }

    public long FkUseridUser { get; set; }

    [JsonIgnore]
    public virtual User FkUseridUserNavigation { get; set; } = null!;
}
