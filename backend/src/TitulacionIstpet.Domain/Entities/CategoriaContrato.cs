using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class CategoriaContrato
{
    public int IdCategoriaContratos { get; set; }

    public string? Nombre { get; set; }

    public virtual ICollection<Escalafon> Escalafons { get; set; } = new List<Escalafon>();
}
