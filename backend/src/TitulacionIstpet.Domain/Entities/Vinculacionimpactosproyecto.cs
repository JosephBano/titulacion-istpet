using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Vinculacionimpactosproyecto
{
    public int IdImpactoproyecto { get; set; }

    public string? ImpactoProyecto { get; set; }

    public bool? Activo { get; set; }

    public virtual ICollection<Vinculacionproyectosimpacto> Vinculacionproyectosimpactos { get; set; } = new List<Vinculacionproyectosimpacto>();
}
