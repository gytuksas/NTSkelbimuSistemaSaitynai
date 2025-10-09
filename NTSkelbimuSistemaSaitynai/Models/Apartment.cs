using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Text.Json.Serialization;

namespace NTSkelbimuSistemaSaitynai.Models;

public partial class Apartment
{
    public int? Apartmentnumber { get; set; }

    public double Area { get; set; }

    public int? Floor { get; set; }

    public int Rooms { get; set; }

    public string? Notes { get; set; }

    public int? Heating { get; set; }

    public int Finish { get; set; }

    public long IdApartment { get; set; }

    public long FkBuildingidBuilding { get; set; }

    public bool Iswholebuilding { get; set; }

    [BindNever]
    [ValidateNever]
    [JsonIgnore]
    public virtual Finishtype FinishNavigation { get; set; } = null!;

    [BindNever]
    [ValidateNever]
    [JsonIgnore]
    public virtual Building FkBuildingidBuildingNavigation { get; set; } = null!;

    [BindNever]
    [ValidateNever]
    [JsonIgnore]
    public virtual Heatingtype? HeatingNavigation { get; set; }

    [BindNever]
    [ValidateNever]
    [JsonIgnore]
    public virtual ICollection<Picture> Pictures { get; set; } = new List<Picture>();
}
