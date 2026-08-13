using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Seddcoevaluacion
{
    public int IdTest { get; set; }

    public int? IdInstrumento { get; set; }

    public string? IdPeriodo { get; set; }

    public int? IdAsignacion { get; set; }

    public string? IdProfesor { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public DateTime? FechaTest { get; set; }

    public virtual Seddinstrumento? IdInstrumentoNavigation { get; set; }

    public virtual ICollection<Sedddetallecoevaluacion> Sedddetallecoevaluacions { get; set; } = new List<Sedddetallecoevaluacion>();
}
