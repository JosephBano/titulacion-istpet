using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class BienConvocatoriasBeca
{
    public int IdConvocatoriasBecas { get; set; }

    public DateOnly FechaInicio { get; set; }

    public DateOnly? FechaFin { get; set; }

    public int? IdDetalleCronograma { get; set; }

    public int IdTipoConvocatoria { get; set; }

    public virtual ICollection<BienPostulacionesBeca> BienPostulacionesBecas { get; set; } = new List<BienPostulacionesBeca>();

    public virtual CronDetalleCronograma? IdDetalleCronogramaNavigation { get; set; }

    public virtual BienTipoConvocatorium IdTipoConvocatoriaNavigation { get; set; } = null!;
}
