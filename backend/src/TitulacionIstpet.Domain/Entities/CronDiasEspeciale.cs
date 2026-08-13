using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class CronDiasEspeciale
{
    public int IdDiasEspeciales { get; set; }

    public DateTime? Fecha { get; set; }

    public int IdTipoDiaEspecial { get; set; }

    public int? IdCronograma { get; set; }

    public bool? EsRecuperable { get; set; }
}
