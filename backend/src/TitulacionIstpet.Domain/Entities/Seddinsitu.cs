using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Seddinsitu
{
    public int IdEvaluacion { get; set; }

    public int? IdInstrumento { get; set; }

    public string? IdPeriodo { get; set; }

    public string? IdProfesor { get; set; }

    public DateTime FechaRegistro { get; set; }

    public decimal? Calificacion { get; set; }

    public string? IdEvaluador { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public int? IdPregunta { get; set; }

    public virtual Seddinstrumento? IdInstrumentoNavigation { get; set; }
}
