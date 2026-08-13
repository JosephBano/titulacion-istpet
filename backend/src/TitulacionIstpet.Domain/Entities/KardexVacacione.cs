using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

/// <summary>
/// Transacciones del Kardex contable de vacaciones (Libro Mayor)
/// </summary>
public partial class KardexVacacione
{
    public int IdKardex { get; set; }

    /// <summary>
    /// Profesor al que se le afecta el saldo
    /// </summary>
    public string IdProfesor { get; set; } = null!;

    /// <summary>
    /// Fecha en que se realiza la transacción
    /// </summary>
    public DateTime FechaTransaccion { get; set; }

    /// <summary>
    /// ASIGNACION_ANUAL, CONSUMO_VACACIONES, AJUSTE_ADMINISTRATIVO, PRESCRIPCION
    /// </summary>
    public string TipoTransaccion { get; set; } = null!;

    /// <summary>
    /// Días afectados: (+) Cargas anuales, (-) Descuentos por consumo o prescripción
    /// </summary>
    public decimal CantidadDias { get; set; }

    /// <summary>
    /// Periodo anual correspondiente (ej. 2024-2025)
    /// </summary>
    public string Periodo { get; set; } = null!;

    /// <summary>
    /// Detalle o justificación contable de la transacción
    /// </summary>
    public string Detalle { get; set; } = null!;

    /// <summary>
    /// Usuario del sistema (TH o RL) que realiza el movimiento
    /// </summary>
    public int UsuarioResponsable { get; set; }

    public virtual Profesore IdProfesorNavigation { get; set; } = null!;

    public virtual Usuario UsuarioResponsableNavigation { get; set; } = null!;
}
