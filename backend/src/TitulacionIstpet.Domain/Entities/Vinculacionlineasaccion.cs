using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Vinculacionlineasaccion
{
    public int IdlineaAsccion { get; set; }

    public string? Linea { get; set; }

    public bool? Activo { get; set; }

    public virtual ICollection<Vinculacionproyecto> Vinculacionproyectos { get; set; } = new List<Vinculacionproyecto>();
}
