using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

/// <summary>
/// Permisos con sueldo y licencias justificadas conforme a la ley
/// </summary>
public partial class SolicitudesLicencia
{
    public int IdLicencia { get; set; }

    /// <summary>
    /// Empleado ausente
    /// </summary>
    public string IdProfesor { get; set; } = null!;

    /// <summary>
    /// Maternidad, Paternidad, Lactancia, Capacitacion, CalamidadDomestica, Fallecimiento
    /// </summary>
    public string TipoLicencia { get; set; } = null!;

    /// <summary>
    /// Inicio de la licencia
    /// </summary>
    public DateOnly FechaInicio { get; set; }

    /// <summary>
    /// Fin de la licencia
    /// </summary>
    public DateOnly FechaFin { get; set; }

    /// <summary>
    /// Cantidad de días solicitados
    /// </summary>
    public int DiasSolicitados { get; set; }

    /// <summary>
    /// Detalle del suceso/solicitud
    /// </summary>
    public string Motivo { get; set; } = null!;

    /// <summary>
    /// Fecha en que ocurrió el hecho
    /// </summary>
    public DateOnly FechaSuceso { get; set; }

    /// <summary>
    /// Fecha de registro en el sistema
    /// </summary>
    public DateTime FechaSolicitud { get; set; }

    /// <summary>
    /// Ruta del justificativo en PDF
    /// </summary>
    public string? RutaDocumentoJustificativo { get; set; }

    /// <summary>
    /// Fecha en que se cargó el justificativo
    /// </summary>
    public DateTime? FechaEntregaJustificativo { get; set; }

    /// <summary>
    /// PendienteJustificacion, PendienteAprobacion, Aprobada, Rechazada, FaltaInjustificada
    /// </summary>
    public string Estado { get; set; } = null!;

    /// <summary>
    /// Usuario de TH que aprueba
    /// </summary>
    public int? UsuarioAprobador { get; set; }

    /// <summary>
    /// Fecha de aprobación de la licencia
    /// </summary>
    public DateTime? FechaAprobacion { get; set; }

    /// <summary>
    /// Detalle del rechazo en caso de aplicar
    /// </summary>
    public string? MotivoRechazo { get; set; }

    public virtual Profesore IdProfesorNavigation { get; set; } = null!;

    public virtual Usuario? UsuarioAprobadorNavigation { get; set; }
}
