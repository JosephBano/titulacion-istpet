using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Vinculaciontiposobjetivo
{
    public int IdTipoObjetivo { get; set; }

    public string? TipoObjetivo { get; set; }

    public bool? EsGeneral { get; set; }

    public bool? Activo { get; set; }
}
