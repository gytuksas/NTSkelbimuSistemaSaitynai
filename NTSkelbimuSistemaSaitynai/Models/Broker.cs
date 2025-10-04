using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NTSkelbimuSistemaSaitynai.Models;

public partial class Broker
{
    public bool Confirmed { get; set; }

    public bool Blocked { get; set; }

    public long IdUser { get; set; }

    [BindNever]
    [ValidateNever]
    [JsonIgnore]
    public virtual ICollection<Availability> Availabilities { get; set; } = new List<Availability>();

    [BindNever]
    [ValidateNever]
    [JsonIgnore]
    public virtual ICollection<Building> Buildings { get; set; } = new List<Building>();

    [BindNever]
    [ValidateNever]
    [JsonIgnore]
    public virtual User IdUserNavigation { get; set; } = null!;
}
