using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class CronCronograma
{
    public int IdCronograma { get; set; }

    public string IdPeriodo { get; set; } = null!;

    public string Detalle { get; set; } = null!;

    public bool? EsPublico { get; set; }

    public bool? EsActivo { get; set; }
}
