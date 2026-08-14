using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class AlumnosTitulo
{
    public string IdAlumno { get; set; } = null!;

    public int IdTitulo { get; set; }

    public DateTime Fecha { get; set; }

    public DateOnly? FechaActa { get; set; }

    public string? NumeroActa { get; set; }

    public string? PrimerVocal { get; set; }

    public string? SegundoVocal { get; set; }

    public string? TercerVocal { get; set; }

    public string? Secretaria { get; set; }

    public string? Rector { get; set; }

    public string? Vicerrector { get; set; }

    public int? TotalCreditos { get; set; }

    public int? TotalAsignaturas { get; set; }

    public int? TotalHoras { get; set; }

    public decimal? PuntajeTotal { get; set; }

    public decimal? NotaFinal { get; set; }

    public string? TituloTesis { get; set; }

    public int? CodigoSistema { get; set; }

    public decimal? PromedioEstudios { get; set; }

    public decimal? NotaTrabajo { get; set; }

    public decimal? NotaDefensa { get; set; }

    public decimal? NotaComplexivo { get; set; }
}
