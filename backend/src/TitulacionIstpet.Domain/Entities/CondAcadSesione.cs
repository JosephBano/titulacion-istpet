using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

/// <summary>
/// conduccion — Sesion de clase del leccionario (grano: dia)
/// </summary>
public partial class CondAcadSesione
{
    public int IdSesion { get; set; }

    /// <summary>
    /// FK asignaciones_profesores.idAsignacion (columna UNIQUE, no la PK compuesta)
    /// </summary>
    public int IdAsignacion { get; set; }

    /// <summary>
    /// FK fechas_horarios.idFecha — el dia de la clase
    /// </summary>
    public int IdFecha { get; set; }

    /// <summary>
    /// Orden de la clase dentro del dia. Normalmente 1.
    /// </summary>
    public bool NumeroBloque { get; set; }

    /// <summary>
    /// Tema general de la clase — el leccionario propiamente dicho
    /// </summary>
    public string Tema { get; set; } = null!;

    /// <summary>
    /// Observaciones generales de la sesion
    /// </summary>
    public string? Observacion { get; set; }

    /// <summary>
    /// cerrada = congelada; solo un inspector puede reabrirla
    /// </summary>
    public string Estado { get; set; } = null!;

    public DateTime? FechaCierre { get; set; }

    public bool? Activo { get; set; }

    /// <summary>
    /// usuarios.idSigafi del docente
    /// </summary>
    public string UsuarioCreacion { get; set; } = null!;

    public DateTime FechaCreacion { get; set; }

    public string? UsuarioActualiza { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public virtual ICollection<CondAcadAsistencia> CondAcadAsistencia { get; set; } = new List<CondAcadAsistencia>();

    public virtual AsignacionesProfesore IdAsignacionNavigation { get; set; } = null!;

    public virtual FechasHorario IdFechaNavigation { get; set; } = null!;
}
