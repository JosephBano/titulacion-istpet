using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class ContratosFactura
{
    public int IdFacturasContratos { get; set; }

    public int IdContratos { get; set; }

    public DateOnly? PeriodoFactura { get; set; }

    public string NumeroFactura { get; set; } = null!;

    public decimal? ValorFacturado { get; set; }

    public virtual Contrato IdContratosNavigation { get; set; } = null!;
}
