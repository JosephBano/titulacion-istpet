using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Vinculacionproyectoshabilidadesblanda
{
    public int IdProyectoHabilidad { get; set; }

    public int? IdProyectoVinculacion { get; set; }

    public int? IdHablidadBlanda { get; set; }

    public int? Orden { get; set; }

    public bool? Activo { get; set; }

    public virtual Vinculacionhabilidadesblanda? IdHablidadBlandaNavigation { get; set; }

    public virtual Vinculacionproyecto? IdProyectoVinculacionNavigation { get; set; }
}
