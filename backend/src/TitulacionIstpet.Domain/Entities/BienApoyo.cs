using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class BienApoyo
{
    public int IdBienApoyo { get; set; }

    public string? Detalle { get; set; }

    public bool? EsBeca { get; set; }

    public bool? EsAyudaEconomica { get; set; }
}
