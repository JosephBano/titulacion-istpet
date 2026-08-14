using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

/// <summary>
/// Tramos laborales por reingresos; base del ciclo vacacional actual
/// </summary>
public partial class ProfesoresHistorialLaboral
{
    public int IdHistorial { get; set; }

    public string IdProfesor { get; set; } = null!;

    public DateOnly FechaIngreso { get; set; }

    /// <summary>
    /// NULL = tramo vigente
    /// </summary>
    public DateOnly? FechaRetiro { get; set; }

    public bool EsTramoActual { get; set; }

    public string? MotivoSalida { get; set; }

    /// <summary>
    /// Contrato asociado al tramo si existe
    /// </summary>
    public int? IdContratos { get; set; }

    public string? Observacion { get; set; }

    public int? RegistradoPorId { get; set; }

    public DateTime FechaRegistro { get; set; }

    public virtual Profesore IdProfesorNavigation { get; set; } = null!;
}
