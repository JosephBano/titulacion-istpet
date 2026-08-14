using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Tiposangre
{
    public string CodigoTipoSangre { get; set; } = null!;

    public string? Grupo { get; set; }

    public bool? SitemaRh { get; set; }

    public virtual ICollection<Profesore> Profesores { get; set; } = [];
}
