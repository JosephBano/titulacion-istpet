using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Seedevaluadoresinsitu
{
    public int IdAsignacionEvaluador { get; set; }

    public string? IdPeriodo { get; set; }

    public string? IdEvaluador { get; set; }

    public string? IdProfesor { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public bool? Activo { get; set; }
}
