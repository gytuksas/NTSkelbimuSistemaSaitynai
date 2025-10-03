using System;
using System.Collections.Generic;
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

    [JsonIgnore]
    public virtual Administrator? Administrator { get; set; }

    [JsonIgnore]
    public virtual Broker? Broker { get; set; }

    [JsonIgnore]
    public virtual Buyer? Buyer { get; set; }

    [JsonIgnore]
    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
}
