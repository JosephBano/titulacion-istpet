using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Vinculacionproyectosimpacto
{
    public int IdProyectoImpacto { get; set; }

    public int? IdProyectoVinculacion { get; set; }

    public int? IdImpactoproyecto { get; set; }

    public bool? Activo { get; set; }

    public virtual Vinculacionimpactosproyecto? IdImpactoproyectoNavigation { get; set; }

    public virtual Vinculacionproyecto? IdProyectoVinculacionNavigation { get; set; }

    public virtual ICollection<Vinculacionproyectosplantrabajo> Vinculacionproyectosplantrabajos { get; set; } = new List<Vinculacionproyectosplantrabajo>();
}
