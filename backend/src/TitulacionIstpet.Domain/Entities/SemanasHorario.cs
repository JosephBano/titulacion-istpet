using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class SemanasHorario
{
    public int IdSemanasHorarios { get; set; }

    public string? Detalle { get; set; }

    public bool? Activo { get; set; }

    public bool? EsExamen { get; set; }
}
