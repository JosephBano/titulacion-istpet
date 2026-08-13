using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Seddpregunta
{
    public int IdPregunta { get; set; }

    public string? Pregunta { get; set; }

    public bool? Activo { get; set; }

    public virtual ICollection<Sedddetalleautoevaluacion> Sedddetalleautoevaluacions { get; set; } = new List<Sedddetalleautoevaluacion>();

    public virtual ICollection<Sedddetallecoevaluacionautoridad> Sedddetallecoevaluacionautoridads { get; set; } = new List<Sedddetallecoevaluacionautoridad>();

    public virtual ICollection<Sedddetallecoevaluacion> Sedddetallecoevaluacions { get; set; } = new List<Sedddetallecoevaluacion>();

    public virtual ICollection<Sedddetalleheteroevaluacion> Sedddetalleheteroevaluacions { get; set; } = new List<Sedddetalleheteroevaluacion>();

    public virtual ICollection<Seddinstrumentospregunta> Seddinstrumentospregunta { get; set; } = new List<Seddinstrumentospregunta>();
}
