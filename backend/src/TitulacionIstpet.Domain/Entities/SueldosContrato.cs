using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class SueldosContrato
{
    public int IdSueldosContratos { get; set; }

    public int IdContratos { get; set; }

    public DateOnly? FechaRegistro { get; set; }

    public DateOnly? FechaCambiosueldo { get; set; }

    public decimal? Sueldo { get; set; }

    public bool? Esactivo { get; set; }

    public string? UsarioRegistra { get; set; }

    public virtual Contrato IdContratosNavigation { get; set; } = null!;
}
