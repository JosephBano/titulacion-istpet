using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class AgendaAcademica
{
    public string? Idperiodo { get; set; }

    public DateOnly? FechaDesde { get; set; }

    public DateOnly? FechaHasta { get; set; }

    public string? Evento { get; set; }
}
