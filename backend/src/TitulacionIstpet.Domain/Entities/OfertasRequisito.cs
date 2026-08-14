using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class OfertasRequisito
{
    public int IdofertasRequisitos { get; set; }

    public int IdofertasLaborales { get; set; }

    public string? Descripcion { get; set; }

    public bool? EsObligatoria { get; set; }

    public virtual OfertasLaborale IdofertasLaboralesNavigation { get; set; } = null!;
}
