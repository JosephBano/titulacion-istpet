using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Escalafon
{
    public int IdEscalafon { get; set; }

    public int IdCategoriaContratos { get; set; }

    public string? Nombre { get; set; }

    public bool? EsActivo { get; set; }

    public virtual ICollection<DedicacionCategoria> DedicacionCategoria { get; set; } = new List<DedicacionCategoria>();

    public virtual CategoriaContrato IdCategoriaContratosNavigation { get; set; } = null!;
}
