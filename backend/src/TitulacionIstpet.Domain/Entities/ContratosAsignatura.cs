using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class ContratosAsignatura
{
    public int IdContratosAsignaturas { get; set; }

    public int IdContratos { get; set; }

    public int IdAsignatura { get; set; }

    public string IdPeriodo { get; set; } = null!;

    public int? Horas { get; set; }

    public bool? EsActivo { get; set; }

    public int? IdAsignacion { get; set; }

    public string? Paralelo { get; set; }

    public int? IdModalidad { get; set; }

    public int? IdSeccion { get; set; }

    public int? IdNivel { get; set; }

    public bool? Pagada { get; set; }

    public decimal? ValorHora { get; set; }

    public virtual Asignatura IdAsignaturaNavigation { get; set; } = null!;

    public virtual Contrato IdContratosNavigation { get; set; } = null!;

    public virtual Periodo IdPeriodoNavigation { get; set; } = null!;
}
