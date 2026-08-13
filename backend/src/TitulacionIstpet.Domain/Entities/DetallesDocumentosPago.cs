using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class DetallesDocumentosPago
{
    public uint Iddocumentopago { get; set; }

    public int Idpago { get; set; }

    public decimal Valor { get; set; }

    public virtual Pago IdpagoNavigation { get; set; } = null!;
}
