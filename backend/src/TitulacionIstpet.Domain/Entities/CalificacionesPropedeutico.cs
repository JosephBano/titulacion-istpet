using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class CalificacionesPropedeutico
{
    public string IdAlumno { get; set; } = null!;

    public int IdAsignatura { get; set; }

    public string IdPeriodo { get; set; } = null!;

    public decimal? Nota1 { get; set; }

    public bool? Aprobado { get; set; }

    public string? Observacion { get; set; }
}
