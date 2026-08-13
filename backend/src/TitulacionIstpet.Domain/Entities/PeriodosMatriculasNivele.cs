using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class PeriodosMatriculasNivele
{
    public string IdPeriodo { get; set; } = null!;

    public int IdNivel { get; set; }

    public int IdSeccion { get; set; }

    public bool? Activo { get; set; }
}
