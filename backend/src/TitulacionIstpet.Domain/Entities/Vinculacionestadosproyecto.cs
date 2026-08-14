using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Vinculacionestadosproyecto
{
    public int IdEstadoProyecto { get; set; }

    public string? Estado { get; set; }

    public int? Orden { get; set; }

    public bool? Activo { get; set; }
}
