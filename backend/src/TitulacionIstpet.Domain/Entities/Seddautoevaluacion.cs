using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Seddautoevaluacion
{
    public int IdTest { get; set; }

    public int? IdInstrumento { get; set; }

    public string? IdPeriodo { get; set; }

    public string? IdProfesor { get; set; }

    public DateTime FechaRegistro { get; set; }

    public virtual Seddinstrumento? IdInstrumentoNavigation { get; set; }

    public virtual ICollection<Sedddetalleautoevaluacion> Sedddetalleautoevaluacions { get; set; } = new List<Sedddetalleautoevaluacion>();
}
