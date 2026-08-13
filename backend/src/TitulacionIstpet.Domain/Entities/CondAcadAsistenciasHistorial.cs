using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

/// <summary>
/// conduccion — Auditoria de cambios de asistencia
/// </summary>
public partial class CondAcadAsistenciasHistorial
{
    public ulong IdHistorial { get; set; }

    public int IdAsistencia { get; set; }

    /// <summary>
    /// Desnormalizado: sobrevive al borrado en cascada
    /// </summary>
    public int IdSesion { get; set; }

    /// <summary>
    /// Desnormalizado por el mismo motivo
    /// </summary>
    public int IdMatricula { get; set; }

    /// <summary>
    /// NULL = alta inicial
    /// </summary>
    public string? EstadoAnterior { get; set; }

    public string EstadoNuevo { get; set; } = null!;

    public string? Motivo { get; set; }

    /// <summary>
    /// usuarios.idSigafi tomado del JWT
    /// </summary>
    public string Usuario { get; set; } = null!;

    /// <summary>
    /// cplec_docente | cplec_inspector
    /// </summary>
    public string? Rol { get; set; }

    public string? IpAddress { get; set; }

    public DateTime Fecha { get; set; }
}
