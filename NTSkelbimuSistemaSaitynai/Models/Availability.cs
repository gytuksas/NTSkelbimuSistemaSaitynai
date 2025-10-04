using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NTSkelbimuSistemaSaitynai.Models;

public partial class Availability
{
    public DateTime From { get; set; }

    public DateTime To { get; set; }

    public long IdAvailability { get; set; }

    public long FkBrokeridUser { get; set; }

    [BindNever]
    [ValidateNever]
    [JsonIgnore]
    public virtual Broker FkBrokeridUserNavigation { get; set; } = null!;

    [BindNever]
    [ValidateNever]
    [JsonIgnore]
    public virtual ICollection<Viewing> Viewings { get; set; } = new List<Viewing>();
}

public class AvailabilityDto
{
    public required string From { get; set; }

    public required string To { get; set; }

    public required long FkBrokeridUser { get; set; }
}
