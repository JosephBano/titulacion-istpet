using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class RbacSistema
{
    public int IdSistema { get; set; }

    public string Codigo { get; set; } = null!;

    public string Detalle { get; set; } = null!;

    public string? Url { get; set; }

    public string? Icono { get; set; }

    public virtual ICollection<RbacModulo> RbacModulos { get; set; } = new List<RbacModulo>();
}
