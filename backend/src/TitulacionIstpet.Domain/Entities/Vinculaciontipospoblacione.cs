using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Vinculaciontipospoblacione
{
    public int IdTipoPoblacion { get; set; }

    public string? TipoPoblacion { get; set; }

    public bool? Activo { get; set; }

    public virtual ICollection<Vinculacionproyectoscarrerasdetalle> Vinculacionproyectoscarrerasdetalles { get; set; } = new List<Vinculacionproyectoscarrerasdetalle>();
}
