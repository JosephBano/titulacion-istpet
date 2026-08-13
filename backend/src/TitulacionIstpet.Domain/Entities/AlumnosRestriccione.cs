using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class AlumnosRestriccione
{
    public string Idalumno { get; set; } = null!;

    public string Idrestriccion { get; set; } = null!;
}
