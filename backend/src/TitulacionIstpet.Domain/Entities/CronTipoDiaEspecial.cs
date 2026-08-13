using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class CronTipoDiaEspecial
{
    public int IdTipoDiaEspecial { get; set; }

    public string? Detalle { get; set; }

    public bool? EsFeriado { get; set; }

    public bool? EsEventoInterno { get; set; }

    public DateOnly? FechaOriginal { get; set; }
}
