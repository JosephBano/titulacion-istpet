using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class TipoContacto
{
    public int IdtipoContacto { get; set; }

    public string? NombreContacto { get; set; }

    public string? LongitudContacto { get; set; }

    public virtual ICollection<EmpresasContacto> EmpresasContactos { get; set; } = [];
}
