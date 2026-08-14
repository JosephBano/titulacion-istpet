using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class CalificacionesGrado
{
    public int IdMatricula { get; set; }

    public int IdAsignatura { get; set; }

    public decimal? Nota { get; set; }

    public bool? Aprobado { get; set; }

    public DateOnly? FechaEvaluacion { get; set; }
}
