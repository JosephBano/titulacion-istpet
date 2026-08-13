using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Vinculacionproyectosprofesore
{
    public int IdProyectoProfesor { get; set; }

    public int? IdProyectoVinculacion { get; set; }

    public string? IdProfesor { get; set; }

    public bool? EsDirector { get; set; }

    public bool? Activo { get; set; }

    public virtual Vinculacionproyecto? IdProyectoVinculacionNavigation { get; set; }
}
