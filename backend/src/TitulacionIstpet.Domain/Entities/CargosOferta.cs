using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class CargosOferta
{
    public int IdcargosOfertas { get; set; }

    public string? NombreCargo { get; set; }

    public virtual ICollection<OfertasLaborale> OfertasLaborales { get; set; } = new List<OfertasLaborale>();
}
