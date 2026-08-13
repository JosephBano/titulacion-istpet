using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class TiposContrato
{
    public int IdTiposContratos { get; set; }

    public string? Nombre { get; set; }

    public string? Codigo { get; set; }

    public int? DuracionSemanas { get; set; }

    public ulong? EsAfiliado { get; set; }

    public virtual ICollection<PlantillaContrato> PlantillaContratos { get; set; } = new List<PlantillaContrato>();
}
