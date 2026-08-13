using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class TiposOferta
{
    public int IdtiposOfertas { get; set; }

    public string? Nombre { get; set; }

    public virtual ICollection<OfertasLaborale> OfertasLaborales { get; set; } = new List<OfertasLaborale>();
}
