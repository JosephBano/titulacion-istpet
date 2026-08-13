using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class CalificacionesConduccion
{
    public int? Idmatricula { get; set; }

    public int? NotaFinal { get; set; }

    public bool? Aprobado { get; set; }

    public string? Observacion { get; set; }
}
