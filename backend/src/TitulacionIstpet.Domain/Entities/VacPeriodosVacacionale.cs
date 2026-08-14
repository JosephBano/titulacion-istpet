using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class VacPeriodosVacacionale
{
    public int IdPeriodoVacaciones { get; set; }

    public string IdProfesor { get; set; } = null!;

    public string OrigenEvento { get; set; } = null!;

    public string PeriodoLectivo { get; set; } = null!;

    public DateOnly FechaInicio { get; set; }

    public DateOnly FechaFin { get; set; }

    public int DiasSolicitados { get; set; }

    public int CantFinesSemana { get; set; }

    public bool EsFueraPlanificacion { get; set; }

    public string Estado { get; set; } = null!;

    public DateTime FechaSolicitud { get; set; }

    public string? RutaDocumento { get; set; }

    public int? UsuarioTh { get; set; }

    public DateTime? FechaAprobacionTh { get; set; }

    public int? UsuarioRl { get; set; }

    public DateTime? FechaAprobacionRl { get; set; }

    public string? MotivoSolicitud { get; set; }

    public string? MotivoRechazo { get; set; }

    public bool RequiereFinSemana { get; set; }

    public int? CantFinesSemanaRequeridos { get; set; }
}
