using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Clausula
{
    public int IdClausulas { get; set; }

    public string? NombreClausula { get; set; }

    public int? Orden { get; set; }

    public bool? EsActivo { get; set; }

    public virtual ICollection<PlantillaClausula> PlantillaClausulas { get; set; } = new List<PlantillaClausula>();
}
