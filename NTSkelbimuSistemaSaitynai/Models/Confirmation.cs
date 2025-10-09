using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Text.Json.Serialization;

namespace NTSkelbimuSistemaSaitynai.Models;

public partial class Confirmation
{
    public string Id { get; set; } = null!;

    public DateTime Expires { get; set; }

    public long FkBuyeridUser { get; set; }

    [BindNever]
    [ValidateNever]
    [JsonIgnore]
    public virtual Buyer FkBuyeridUserNavigation { get; set; } = null!;
}

public class ConfirmationDto
{
    public required string Expires { get; set; }

    public required long FkBuyeridUser { get; set; }
}
