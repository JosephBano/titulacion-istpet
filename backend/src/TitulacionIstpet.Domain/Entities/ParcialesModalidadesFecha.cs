using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class ParcialesModalidadesFecha
{
    public string? IdPeriodo { get; set; }

    public int? IdParcial { get; set; }

    public int? IdModalidad { get; set; }

    public DateOnly? FechaInicio { get; set; }

    public DateOnly? FechaFin { get; set; }

    public bool? Activo { get; set; }
}
