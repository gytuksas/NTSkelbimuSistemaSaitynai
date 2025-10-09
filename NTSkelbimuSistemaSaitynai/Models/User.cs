using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Text.Json.Serialization;

namespace NTSkelbimuSistemaSaitynai.Models;

public partial class User
{
    public string Name { get; set; } = null!;

    public string Surname { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Password { get; set; } = null!;

    public DateTime Registrationtime { get; set; }

    public string? Profilepicture { get; set; }

    public long IdUser { get; set; }

    [BindNever]
    [ValidateNever]
    [JsonIgnore]
    public virtual Administrator? Administrator { get; set; }

    [BindNever]
    [ValidateNever]
    [JsonIgnore]
    public virtual Broker? Broker { get; set; }

    [BindNever]
    [ValidateNever]
    [JsonIgnore]
    public virtual Buyer? Buyer { get; set; }

    [BindNever]
    [ValidateNever]
    [JsonIgnore]
    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
}

public class UserDto
{
    public required string Name { get; set; } = null!;

    public required string Surname { get; set; } = null!;

    public required string Email { get; set; } = null!;

    public required string Phone { get; set; } = null!;

    public required string Password { get; set; } = null!;

    public required string Registrationtime { get; set; }

    public string? Profilepicture { get; set; }
}
