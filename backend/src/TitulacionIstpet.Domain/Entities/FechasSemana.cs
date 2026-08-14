using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class FechasSemana
{
    public int IdFechasSemanas { get; set; }

    public int IdSemanasHorarios { get; set; }

    public int IdFecha { get; set; }

    public string IdPeriodo { get; set; } = null!;
}
