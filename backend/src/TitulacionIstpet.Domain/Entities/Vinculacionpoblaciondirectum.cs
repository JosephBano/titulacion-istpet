using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Vinculacionpoblaciondirectum
{
    public int IdPoblacionDirecta { get; set; }

    public string? Directa { get; set; }

    public bool Activo { get; set; }
}
