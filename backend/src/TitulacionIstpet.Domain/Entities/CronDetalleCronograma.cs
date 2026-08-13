using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class CronDetalleCronograma
{
    public int IdDetalleCronograma { get; set; }

    public int IdCronograma { get; set; }

    public int IdTipoProceso { get; set; }

    public DateTime FechaInicio { get; set; }

    public DateTime FechaFin { get; set; }

    public DateTime? FechaExtension { get; set; }

    public string? Observaciones { get; set; }

    public virtual ICollection<BienConvocatoriasBeca> BienConvocatoriasBecas { get; set; } = new List<BienConvocatoriasBeca>();
}
