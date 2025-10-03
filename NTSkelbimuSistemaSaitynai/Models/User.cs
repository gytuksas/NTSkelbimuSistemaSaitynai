using System;
using System.Collections.Generic;

namespace NTSkelbimuSistemaSaitynai.Models;

public partial class User
{
    public string Name { get; set; } = null!;

    public string Surname { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Password { get; set; } = null!;

    public DateOnly Registrationtime { get; set; }

    public string? Profilepicture { get; set; }

    public long IdUser { get; set; }

    public virtual Administrator? Administrator { get; set; }

    public virtual Broker? Broker { get; set; }

    public virtual Buyer? Buyer { get; set; }

    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
}
