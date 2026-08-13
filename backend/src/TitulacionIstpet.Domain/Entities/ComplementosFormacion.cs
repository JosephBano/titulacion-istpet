using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class ComplementosFormacion
{
    public int IdComplemento { get; set; }

    public string? Complemento { get; set; }

    public bool? Activo { get; set; }
}
