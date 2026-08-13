using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class CronTipoProceso
{
    public int IdTipoProceso { get; set; }

    public string Detalle { get; set; } = null!;

    public bool? EsInformativo { get; set; }

    public string Audiencia { get; set; } = null!;

    public int? Orden { get; set; }

    public bool? EsActivo { get; set; }
}
