using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Restriccione
{
    public string Idrestriccion { get; set; } = null!;

    public string? Restriccion { get; set; }

    public ulong? Activo { get; set; }
}
