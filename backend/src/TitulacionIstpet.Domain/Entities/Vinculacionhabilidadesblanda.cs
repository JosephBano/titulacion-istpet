using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Vinculacionhabilidadesblanda
{
    public int IdHablidadBlanda { get; set; }

    public string? HabilidadBlanda { get; set; }

    public string? Descripcion { get; set; }

    public bool? Activo { get; set; }

    public virtual ICollection<Vinculacionproyectoshabilidadesblanda> Vinculacionproyectoshabilidadesblanda { get; set; } = new List<Vinculacionproyectoshabilidadesblanda>();
}
