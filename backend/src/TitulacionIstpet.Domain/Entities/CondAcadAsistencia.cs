using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

/// <summary>
/// conduccion — Asistencia por estudiante y sesion
/// </summary>
public partial class CondAcadAsistencia
{
    public int IdAsistencia { get; set; }

    /// <summary>
    /// FK cplec_sesiones.idSesion
    /// </summary>
    public int IdSesion { get; set; }

    /// <summary>
    /// FK matriculas.idMatricula
    /// </summary>
    public int IdMatricula { get; set; }

    public string Estado { get; set; } = null!;

    /// <summary>
    /// Solo aplica cuando estado = atraso
    /// </summary>
    public ushort? MinutosAtraso { get; set; }

    public string? Observacion { get; set; }

    /// <summary>
    /// usuarios.idSigafi del autor
    /// </summary>
    public string UsuarioCreacion { get; set; } = null!;

    public DateTime FechaCreacion { get; set; }

    public string? UsuarioActualiza { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public virtual Matricula IdMatriculaNavigation { get; set; } = null!;

    public virtual CondAcadSesione IdSesionNavigation { get; set; } = null!;
}
