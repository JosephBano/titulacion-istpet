using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Vinculacionpoblacionindirectum
{
    public int IdPoblacionIndirecta { get; set; }

    public string? Indirecta { get; set; }

    public bool Activo { get; set; }
}
