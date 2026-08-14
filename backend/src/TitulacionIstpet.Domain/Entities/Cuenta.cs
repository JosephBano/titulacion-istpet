using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Cuenta
{
    public int IdCuenta { get; set; }

    public string Cuenta1 { get; set; } = null!;

    public string NumeroCuenta { get; set; } = null!;

    public bool Activo { get; set; }

    public byte Esingreso { get; set; }

    public string? TipoPago { get; set; }
}
