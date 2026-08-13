using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class RbacModulo
{
    public int IdModulos { get; set; }

    public int IdSistema { get; set; }

    public string? Nombre { get; set; }

    public bool? EsActivo { get; set; }

    public virtual RbacSistema IdSistemaNavigation { get; set; } = null!;

    public virtual ICollection<RbacModulosOperacione> RbacModulosOperaciones { get; set; } = [];
}
