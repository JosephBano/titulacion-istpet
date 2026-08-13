using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class RbacRolModuloOperacion
{
    public int IdRolModuloOperacion { get; set; }

    public int IdModulosOperaciones { get; set; }

    public int IdRol { get; set; }

    public DateOnly? FechaAsignacion { get; set; }

    public DateOnly? FechaModificacion { get; set; }

    public DateOnly? FechaDesactivacion { get; set; }

    public bool? EsActivo { get; set; }

    public virtual RbacModulosOperacione IdModulosOperacionesNavigation { get; set; } = null!;

    public virtual RbacRol IdRolNavigation { get; set; } = null!;
}
