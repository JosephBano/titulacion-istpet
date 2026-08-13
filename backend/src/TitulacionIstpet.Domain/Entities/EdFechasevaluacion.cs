using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class EdFechasevaluacion
{
    public string IdPeriodo { get; set; } = null!;

    public int IdModalidad { get; set; }

    public DateOnly? FechaInicio { get; set; }

    public DateOnly? FechaFinal { get; set; }
}
