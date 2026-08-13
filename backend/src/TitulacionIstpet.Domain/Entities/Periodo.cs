using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Periodo
{
    public string IdPeriodo { get; set; } = null!;

    public string? Detalle { get; set; }

    public DateOnly? FechaInicial { get; set; }

    public DateOnly? FechaFinal { get; set; }

    public bool? Cerrado { get; set; }

    public DateOnly? FechaMaximaAutocierre { get; set; }

    public bool? Activo { get; set; }

    public bool? Creditos { get; set; }

    public uint? NumeroPagos { get; set; }

    public DateOnly? FechaMatruclaExtraordinaria { get; set; }

    public int? Foliop { get; set; }

    public bool? PermiteMatricula { get; set; }

    public bool? IngresoCalificaciones { get; set; }

    public bool? PermiteCalificacionesInstituto { get; set; }

    public bool? Periodoactivoinstituto { get; set; }

    public bool? VisualizaPowerBi { get; set; }

    public bool? EsInstituto { get; set; }

    public bool? PeriodoPlanificacion { get; set; }

    public bool? EsConduccion { get; set; }

    public virtual ICollection<BienCaso> BienCasos { get; set; } = [];

    public virtual ICollection<BienResolucionesTribunale> BienResolucionesTribunales { get; set; } = [];

    public virtual ICollection<ContratosAsignatura> ContratosAsignaturas { get; set; } = [];

    public virtual ICollection<FechasPagosCuota> FechasPagosCuota { get; set; } = [];

    public virtual ICollection<Matricula> Matriculas { get; set; } = [];

    public virtual ICollection<ProfesoresCarrerasPeriodo> ProfesoresCarrerasPeriodos { get; set; } = [];

    public virtual ICollection<ProfesoresDedicacion> ProfesoresDedicacions { get; set; } = [];
}
