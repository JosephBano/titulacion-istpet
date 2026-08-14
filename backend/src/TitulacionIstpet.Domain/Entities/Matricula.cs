using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Matricula
{
    public int IdMatricula { get; set; }

    public string IdAlumno { get; set; } = null!;

    public int IdNivel { get; set; }

    public int IdSeccion { get; set; }

    public int IdModalidad { get; set; }

    public string IdPeriodo { get; set; } = null!;

    public DateTime? FechaMatricula { get; set; }

    public string? Paralelo { get; set; }

    public bool? Arrastres { get; set; }

    public int? Folio { get; set; }

    public decimal? BecaMatricula { get; set; }

    public decimal? BecaColegiatura { get; set; }

    public bool? Retirado { get; set; }

    public DateOnly? FechaRetiro { get; set; }

    public string? Observacion { get; set; }

    public bool? Convalidacion { get; set; }

    public string? CarreraConvalidada { get; set; }

    public int? NumeroPermiso { get; set; }

    public string? UserMatricula { get; set; }

    public bool? Valida { get; set; }

    public bool? EsOyente { get; set; }

    public string? DocumentoFactura { get; set; }

    public virtual ICollection<BienApoyoFinanciero> BienApoyoFinancieros { get; set; } = [];

    public virtual ICollection<BienPostulacionesBeca> BienPostulacionesBecas { get; set; } = [];

    public virtual ICollection<Calificacione> Calificaciones { get; set; } = [];

    public virtual ICollection<CondAcadAsistencia> CondAcadAsistencia { get; set; } = [];

    public virtual Alumno IdAlumnoNavigation { get; set; } = null!;

    public virtual Modalidade IdModalidadNavigation { get; set; } = null!;

    public virtual Curso IdNivelNavigation { get; set; } = null!;

    public virtual Periodo IdPeriodoNavigation { get; set; } = null!;

    public virtual Seccione IdSeccionNavigation { get; set; } = null!;

    public virtual ICollection<Solicitudescalificacione> Solicitudescalificaciones { get; set; } = [];

    public virtual ICollection<Vinculacionproyectosalumno> Vinculacionproyectosalumnos { get; set; } = [];
}
