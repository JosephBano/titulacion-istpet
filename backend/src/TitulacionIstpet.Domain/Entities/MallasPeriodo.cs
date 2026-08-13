using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class MallasPeriodo
{
    public string IdPeriodo { get; set; } = null!;

    public int IdNivel { get; set; }

    public int IdMalla { get; set; }
}
