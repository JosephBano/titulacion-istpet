using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Seddcoevaluacionautoridad
{
    public int IdTest { get; set; }

    public int? IdInstrumento { get; set; }

    public string? IdPeriodo { get; set; }

    public string? IdProfesor { get; set; }

    public string? IdEvaluador { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public DateTime? FechaTest { get; set; }

    public virtual Seddinstrumento? IdInstrumentoNavigation { get; set; }

    public virtual ICollection<Sedddetallecoevaluacionautoridad> Sedddetallecoevaluacionautoridads { get; set; } = [];
}
