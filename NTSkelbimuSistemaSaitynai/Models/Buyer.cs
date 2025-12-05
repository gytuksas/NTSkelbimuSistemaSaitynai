using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NTSkelbimuSistemaSaitynai.Models;

public partial class Buyer
{
    public bool Confirmed { get; set; }

    public bool Blocked { get; set; }

    public long IdUser { get; set; }

    [BindNever]
    [ValidateNever]
    [JsonIgnore]
    public virtual Confirmation? Confirmation { get; set; }

    [BindNever]
    [ValidateNever]
    [JsonIgnore]
    public virtual User IdUserNavigation { get; set; } = null!;

    [BindNever]
    [ValidateNever]
    [JsonIgnore]
    public virtual ICollection<Viewing> Viewings { get; set; } = new List<Viewing>();
}

public sealed class BuyerPatchDto
{
    public bool? Confirmed { get; set; }
    public bool? Blocked { get; set; }
}
