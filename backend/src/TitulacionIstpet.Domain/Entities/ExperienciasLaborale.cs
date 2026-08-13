using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class ExperienciasLaborale
{
    public int IdexperienciasLaborales { get; set; }

    public string? IdAlumno { get; set; }

    public string? EmpresaNombre { get; set; }

    public string? PuestoNombre { get; set; }

    public DateOnly? FechaInicio { get; set; }

    public DateOnly? FechaFin { get; set; }

    public string? Descripcion { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public virtual Alumno? IdAlumnoNavigation { get; set; }
}
