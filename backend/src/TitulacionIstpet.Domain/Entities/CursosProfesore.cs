using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class CursosProfesore
{
    public int IdCursoProfesor { get; set; }

    public string IdProfesor { get; set; } = null!;

    public string? NombreCurso { get; set; }

    public string? Institucion { get; set; }

    public DateOnly? FechaInicio { get; set; }

    public DateOnly? FechaFinalizacion { get; set; }

    public int? NumeroHoras { get; set; }

    public bool? EsValido { get; set; }

    public string? ArchivoCurso { get; set; }

    public bool? FinancioInstituto { get; set; }

    public virtual Profesore IdProfesorNavigation { get; set; } = null!;
}
