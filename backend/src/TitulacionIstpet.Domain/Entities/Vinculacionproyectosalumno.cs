using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Vinculacionproyectosalumno
{
    public int IdProyectoAlumno { get; set; }

    public int? IdProyectoVinculacion { get; set; }

    public int? IdMatricula { get; set; }

    public bool? Activo { get; set; }

    public virtual Matricula? IdMatriculaNavigation { get; set; }

    public virtual Vinculacionproyecto? IdProyectoVinculacionNavigation { get; set; }
}
