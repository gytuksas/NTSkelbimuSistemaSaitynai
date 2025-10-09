using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Text.Json.Serialization;

namespace NTSkelbimuSistemaSaitynai.Models;

public partial class Picture
{
    public string Id { get; set; } = null!;

    public bool Public { get; set; }

    public long FkApartmentidApartment { get; set; }

    [BindNever]
    [ValidateNever]
    [JsonIgnore]
    public virtual Apartment FkApartmentidApartmentNavigation { get; set; } = null!;

    [BindNever]
    [ValidateNever]
    [JsonIgnore]
    public virtual Listing? Listing { get; set; }
}

public class PicturePublicPatchDto
{
    public required bool Public { get; set; }
}