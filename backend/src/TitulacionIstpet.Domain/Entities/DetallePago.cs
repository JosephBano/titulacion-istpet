using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class DetallePago
{
    public int IdPago { get; set; }

    public int IdEspecie { get; set; }

    public decimal? Valor { get; set; }

    public decimal? Descuento { get; set; }

    public int? IdCredito { get; set; }

    public bool? MigradoContabilidad { get; set; }

    public DateTime? FechaMigracion { get; set; }

    public virtual Especy IdEspecieNavigation { get; set; } = null!;

    public virtual Pago IdPagoNavigation { get; set; } = null!;
}
