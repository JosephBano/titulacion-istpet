using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Seddheteroevaluacion
{
    public int IdTest { get; set; }

    public int? IdInstrumento { get; set; }

    public string? IdPeriodo { get; set; }

    public int? IdAsignacion { get; set; }

    public int? IdMatricula { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public virtual Seddinstrumento? IdInstrumentoNavigation { get; set; }

    public virtual ICollection<Sedddetalleheteroevaluacion> Sedddetalleheteroevaluacions { get; set; } = [];
}
