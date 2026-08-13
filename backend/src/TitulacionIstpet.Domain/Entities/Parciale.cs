using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Parciale
{
    public int IdParcial { get; set; }

    public string? Parcial { get; set; }

    public DateOnly? FechaInicio { get; set; }

    public DateOnly? FechaFinal { get; set; }

    public bool? EsPrimero { get; set; }

    public bool? EsSegundo { get; set; }

    public bool? EsExamenFinal { get; set; }

    public bool? EsRemedial { get; set; }

    public virtual ICollection<Solicitudescalificacione> Solicitudescalificaciones { get; set; } = new List<Solicitudescalificacione>();
}
