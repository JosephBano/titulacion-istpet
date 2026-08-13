using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Dedicacion
{
    public int IdDedicacion { get; set; }

    public string? Nombre { get; set; }

    public virtual ICollection<DedicacionCategoria> DedicacionCategoria { get; set; } = new List<DedicacionCategoria>();

    public virtual ICollection<HorasAcademica> HorasAcademicas { get; set; } = new List<HorasAcademica>();

    public virtual ICollection<PlantillaContrato> PlantillaContratos { get; set; } = new List<PlantillaContrato>();
}
