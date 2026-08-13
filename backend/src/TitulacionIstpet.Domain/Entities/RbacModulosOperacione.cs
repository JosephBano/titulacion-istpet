using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class RbacModulosOperacione
{
    public int IdModulosOperaciones { get; set; }

    public int IdModulos { get; set; }

    public int IdOperaciones { get; set; }

    public DateOnly? FechaCreacion { get; set; }

    public DateOnly? FechaModificacion { get; set; }

    public bool? EsActivo { get; set; }

    public virtual RbacModulo IdModulosNavigation { get; set; } = null!;

    public virtual RbacOperacione IdOperacionesNavigation { get; set; } = null!;

    public virtual ICollection<RbacRolModuloOperacion> RbacRolModuloOperacions { get; set; } = [];
}
