using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Dedicacion
{
    public int IdDedicacion { get; set; }

    public string? Nombre { get; set; }

    public virtual ICollection<DedicacionCategoria> DedicacionCategoria { get; set; } = [];

    public virtual ICollection<HorasAcademica> HorasAcademicas { get; set; } = [];

    public virtual ICollection<PlantillaContrato> PlantillaContratos { get; set; } = [];
}
