using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class AlumnosActaConduccion
{
    public string Idalumno { get; set; } = null!;

    public int? NumeroActa { get; set; }

    public DateOnly? FechaGrado { get; set; }

    public string Idperiodo { get; set; } = null!;
}
