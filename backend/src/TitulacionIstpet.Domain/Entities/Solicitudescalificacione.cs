using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Solicitudescalificacione
{
    public int IdSolicitudCalificacion { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public DateTime? FechaHabilitado { get; set; }

    public int? IdSolicitud { get; set; }

    public int? IdParcial { get; set; }

    public int? IdMatricula { get; set; }

    public int? IdAsignatura { get; set; }

    public int? IdNivel { get; set; }

    public string? IdPeriodo { get; set; }

    public string? Paralelo { get; set; }

    public DateTime? FechaCalificacion { get; set; }

    public string? IdProfesor { get; set; }

    public decimal? Calificacion { get; set; }

    public bool? Activo { get; set; }

    public virtual Asignatura? IdAsignaturaNavigation { get; set; }

    public virtual Matricula? IdMatriculaNavigation { get; set; }

    public virtual Curso? IdNivelNavigation { get; set; }

    public virtual Parciale? IdParcialNavigation { get; set; }

    public virtual Solicitude? IdSolicitudNavigation { get; set; }
}
