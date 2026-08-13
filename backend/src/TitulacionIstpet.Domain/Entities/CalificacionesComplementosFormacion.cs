using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class CalificacionesComplementosFormacion
{
    public string IdAlumno { get; set; } = null!;

    public int IdComplemento { get; set; }

    public int IdAsignatura { get; set; }

    public decimal? NotaFinal { get; set; }

    public bool? Aprobado { get; set; }
}
