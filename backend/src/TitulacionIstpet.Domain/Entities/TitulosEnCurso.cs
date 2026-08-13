using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class TitulosEnCurso
{
    public int IdTitulosProfesorCurso { get; set; }

    public string IdProfesor { get; set; } = null!;

    public string? Titulo { get; set; }

    public int IdUniversidad { get; set; }

    public int IdGradoAcademico { get; set; }

    public int IdCampoDetalladoUnesco { get; set; }

    public DateOnly? FechaInicio { get; set; }

    public bool? TieneBeca { get; set; }

    public int? PorcentajeBeca { get; set; }

    public int? IdTipoBeca { get; set; }

    public decimal? MontoBeca { get; set; }

    public int? IdFinanciamiento { get; set; }

    public string? NombreOtro { get; set; }

    public virtual CampoDetalladoUnesco IdCampoDetalladoUnescoNavigation { get; set; } = null!;

    public virtual FinanciamientoBeca? IdFinanciamientoNavigation { get; set; }

    public virtual GradosAcademico IdGradoAcademicoNavigation { get; set; } = null!;

    public virtual Profesore IdProfesorNavigation { get; set; } = null!;

    public virtual TiposBeca? IdTipoBecaNavigation { get; set; }

    public virtual Universidade IdUniversidadNavigation { get; set; } = null!;
}
