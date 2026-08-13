using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class BienDatosEducacion
{
    public int IdDatosEducacion { get; set; }

    public string NombreCurso { get; set; } = null!;

    public string? NombreInstitucion { get; set; }

    public bool EsIstpet { get; set; }

    public string NivelEducacion { get; set; } = null!;

    public bool EstaCursando { get; set; }

    public bool EsPresencial { get; set; }

    public bool EsBecado { get; set; }

    public string TipoEducacion { get; set; } = null!;

    public string IdAlumno { get; set; } = null!;

    public virtual Alumno IdAlumnoNavigation { get; set; } = null!;
}
