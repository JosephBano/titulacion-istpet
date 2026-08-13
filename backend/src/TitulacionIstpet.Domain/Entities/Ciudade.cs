using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Ciudade
{
    public int Idciudades { get; set; }

    public int Idprovincias { get; set; }

    public string? Nombre { get; set; }

    public virtual Provincia IdprovinciasNavigation { get; set; } = null!;

    public virtual ICollection<Parroquia> Parroquia { get; set; } = new List<Parroquia>();
}
