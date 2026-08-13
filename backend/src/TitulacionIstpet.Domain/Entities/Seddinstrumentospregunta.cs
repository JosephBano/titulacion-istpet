using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Seddinstrumentospregunta
{
    public int IdInstrumentoPregunta { get; set; }

    public int? IdInstrumento { get; set; }

    public int? IdPregunta { get; set; }

    public DateTime FechaRegistro { get; set; }

    public bool? Activo { get; set; }

    public virtual Seddinstrumento? IdInstrumentoNavigation { get; set; }

    public virtual Seddpregunta? IdPreguntaNavigation { get; set; }
}
