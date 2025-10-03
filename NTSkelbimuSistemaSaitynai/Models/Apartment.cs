using System;
using System.Collections.Generic;

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

    public virtual Finishtype FinishNavigation { get; set; } = null!;

    public virtual Building FkBuildingidBuildingNavigation { get; set; } = null!;

    public virtual Heatingtype? HeatingNavigation { get; set; }

    public virtual ICollection<Picture> Pictures { get; set; } = new List<Picture>();
}
