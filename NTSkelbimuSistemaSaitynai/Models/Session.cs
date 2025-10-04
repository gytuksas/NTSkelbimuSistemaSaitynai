using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
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

    [BindNever]
    [ValidateNever]
    [JsonIgnore]
    public virtual User FkUseridUserNavigation { get; set; } = null!;
}

public class SessionDto
{
    public required string Created { get; set; }

    public required bool Remember { get; set; }

    public required string Lastactivity { get; set; }

    public required long FkUseridUser { get; set; }
}
