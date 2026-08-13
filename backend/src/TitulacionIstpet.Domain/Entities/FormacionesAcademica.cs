using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class FormacionesAcademica
{
    public int IdformacionesAcademicas { get; set; }

    public string IdAlumno { get; set; } = null!;

    public string? InstitucionNombre { get; set; }

    public string? Titulo { get; set; }

    public string? Abreviatura { get; set; }

    public string? NumeroRegistro { get; set; }

    public string? AreaEstudio { get; set; }

    public DateOnly? FechaInicio { get; set; }

    public DateOnly? FechaFin { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }
}
