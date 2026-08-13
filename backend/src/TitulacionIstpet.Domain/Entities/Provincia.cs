using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Provincia
{
    public int Idprovincias { get; set; }

    public int Idpaises { get; set; }

    public string? Nombre { get; set; }

    public virtual ICollection<Ciudade> Ciudades { get; set; } = new List<Ciudade>();

    public virtual Paise IdpaisesNavigation { get; set; } = null!;
}
