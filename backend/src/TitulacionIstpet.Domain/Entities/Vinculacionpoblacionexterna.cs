using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Vinculacionpoblacionexterna
{
    public int IdPoblacionExterna { get; set; }

    public string? Externa { get; set; }

    public bool Activo { get; set; }
}
