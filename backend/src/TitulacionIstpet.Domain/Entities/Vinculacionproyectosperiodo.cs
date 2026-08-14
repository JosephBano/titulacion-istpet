using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Vinculacionproyectosperiodo
{
    public int IdProyectoPeriodo { get; set; }

    public int? IdProyectoVinculacion { get; set; }

    public string? IdPeriodo { get; set; }

    public bool? EsPrincipal { get; set; }

    public bool? Activo { get; set; }

    public virtual Vinculacionproyecto? IdProyectoVinculacionNavigation { get; set; }
}
