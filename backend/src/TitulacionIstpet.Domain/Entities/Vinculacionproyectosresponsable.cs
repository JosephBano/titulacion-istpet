using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Vinculacionproyectosresponsable
{
    public int IdProyectoResponsable { get; set; }

    public int? IdProyectoVinculacion { get; set; }

    public string? IdProfesor { get; set; }

    public bool? EsColaborador { get; set; }

    public bool? Activo { get; set; }

    public virtual Vinculacionproyecto? IdProyectoVinculacionNavigation { get; set; }
}
