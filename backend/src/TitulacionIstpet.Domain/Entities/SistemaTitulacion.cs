using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class SistemaTitulacion
{
    public int CodigoSistema { get; set; }

    public string? Detalle { get; set; }

    public bool? Activo { get; set; }
}
