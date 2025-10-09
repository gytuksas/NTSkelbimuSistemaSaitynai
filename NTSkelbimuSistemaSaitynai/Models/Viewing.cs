using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Text.Json.Serialization;

namespace NTSkelbimuSistemaSaitynai.Models;

public partial class Viewing
{
    public long IdViewing { get; set; }

    public DateTime From { get; set; }

    public DateTime To { get; set; }

    public int Status { get; set; }

    public long FkAvailabilityidAvailability { get; set; }

    public long FkListingidListing { get; set; }

    [BindNever]
    [ValidateNever]
    [JsonIgnore]
    public virtual Availability FkAvailabilityidAvailabilityNavigation { get; set; } = null!;

    [BindNever]
    [ValidateNever]
    [JsonIgnore]
    public virtual Listing FkListingidListingNavigation { get; set; } = null!;

    [BindNever]
    [ValidateNever]
    [JsonIgnore]
    public virtual Viewingstatus StatusNavigation { get; set; } = null!;
}

public class ViewingDto
{
    public required string From { get; set; }

    public required string To { get; set; }

    public required int Status { get; set; }

    public long FkAvailabilityidAvailability { get; set; }

    public long FkListingidListing { get; set; }
}

public class ViewingPatchDto
{
    public required int Status { get; set; }
}