using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Vinculacionproyectosobjetivo
{
    public int IdProyectoObjetivo { get; set; }

    public int? IdProyectoVinculacion { get; set; }

    public string? Objetivo { get; set; }

    public bool? EsGeneral { get; set; }

    public string? Resultado { get; set; }

    public int? Orden { get; set; }

    public bool? Activo { get; set; }

    public virtual Vinculacionproyecto? IdProyectoVinculacionNavigation { get; set; }

    public virtual ICollection<Vinculacionproyectosplantrabajo> Vinculacionproyectosplantrabajos { get; set; } = [];
}
