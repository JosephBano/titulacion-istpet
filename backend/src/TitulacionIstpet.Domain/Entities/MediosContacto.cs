using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class MediosContacto
{
    public int IdMedio { get; set; }

    public string? Medio { get; set; }

    public ulong? Activo { get; set; }
}
