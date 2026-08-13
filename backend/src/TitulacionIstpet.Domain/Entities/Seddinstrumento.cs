using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Seddinstrumento
{
    public int IdInstrumento { get; set; }

    public int? IdCategoria { get; set; }

    public string? Instrumento { get; set; }

    public string? Codigo { get; set; }

    public int? Porcentaje { get; set; }

    public bool? Activo { get; set; }

    public virtual CategoriasActividade? IdCategoriaNavigation { get; set; }

    public virtual ICollection<Seddautoevaluacion> Seddautoevaluacions { get; set; } = new List<Seddautoevaluacion>();

    public virtual ICollection<Seddcoevaluacionautoridad> Seddcoevaluacionautoridads { get; set; } = new List<Seddcoevaluacionautoridad>();

    public virtual ICollection<Seddcoevaluacion> Seddcoevaluacions { get; set; } = new List<Seddcoevaluacion>();

    public virtual ICollection<Seddheteroevaluacion> Seddheteroevaluacions { get; set; } = new List<Seddheteroevaluacion>();

    public virtual ICollection<Seddinsitu> Seddinsitus { get; set; } = new List<Seddinsitu>();

    public virtual ICollection<Seddinstrumentospregunta> Seddinstrumentospregunta { get; set; } = new List<Seddinstrumentospregunta>();
}
