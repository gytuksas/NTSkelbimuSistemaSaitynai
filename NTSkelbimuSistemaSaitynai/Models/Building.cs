using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NTSkelbimuSistemaSaitynai.Models;

public partial class Building
{
    public string City { get; set; } = null!;

    public string Address { get; set; } = null!;

    public double Area { get; set; }

    public int Year { get; set; }

    public int? Lastrenovationyear { get; set; }

    public int Floors { get; set; }

    public int? Energy { get; set; }

    public long IdBuilding { get; set; }

    public long FkBrokeridUser { get; set; }

    [BindNever]
    [ValidateNever]
    [JsonIgnore]
    public virtual ICollection<Apartment> Apartments { get; set; } = new List<Apartment>();

    [BindNever]
    [ValidateNever]
    [JsonIgnore]
    public virtual Energyclass? EnergyNavigation { get; set; }

    [BindNever]
    [ValidateNever]
    [JsonIgnore]
    public virtual Broker FkBrokeridUserNavigation { get; set; } = null!;
}
