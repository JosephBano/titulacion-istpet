using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Calificacione
{
    public int IdMatricula { get; set; }

    public int IdAsignatura { get; set; }

    public int? IdNivel { get; set; }

    public string? Paralelo { get; set; }

    public int? IdSeccion { get; set; }

    public int? IdModalidad { get; set; }

    public decimal? Ef1 { get; set; }

    public decimal? Ep1 { get; set; }

    public decimal? Nota1 { get; set; }

    public int? Faltasi1 { get; set; }

    public int? Faltasj1 { get; set; }

    public decimal? Ef2 { get; set; }

    public decimal? Ep2 { get; set; }

    public decimal? Nota2 { get; set; }

    public int? Faltasi2 { get; set; }

    public int? Faltasj2 { get; set; }

    public decimal? Nota3 { get; set; }

    public int? Faltasi3 { get; set; }

    public int? Faltasj3 { get; set; }

    public decimal? Nota4 { get; set; }

    public int? Faltasi4 { get; set; }

    public int? Faltasj4 { get; set; }

    public decimal? Nota5 { get; set; }

    public int? HorasAsistidas { get; set; }

    public decimal? RemedialParcial { get; set; }

    public decimal? PromedioParcial { get; set; }

    public decimal? Examen { get; set; }

    public decimal? RemedialFinal { get; set; }

    public decimal? PromedioFinal { get; set; }

    public decimal? NotaFinal { get; set; }

    public bool? Aprobado { get; set; }

    public bool? Remedial { get; set; }

    public string? Observacion { get; set; }

    public string? Tipo { get; set; }

    public bool? PierdeFaltas { get; set; }

    public string? CodigoSolicitud { get; set; }

    public DateOnly? FechaMaximaRemedial { get; set; }

    public virtual Asignatura IdAsignaturaNavigation { get; set; } = null!;

    public virtual Matricula IdMatriculaNavigation { get; set; } = null!;
}
