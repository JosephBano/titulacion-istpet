using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class EdEncuesta
{
    public int IdEncuesta { get; set; }

    public string? Nombre { get; set; }

    public string? Descripcion { get; set; }

    public bool? Activo { get; set; }
}
