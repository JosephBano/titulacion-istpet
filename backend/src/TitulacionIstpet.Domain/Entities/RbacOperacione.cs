using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class RbacOperacione
{
    public int IdOperaciones { get; set; }

    public string? NombreOperacion { get; set; }

    public virtual ICollection<RbacModulosOperacione> RbacModulosOperaciones { get; set; } = new List<RbacModulosOperacione>();
}
