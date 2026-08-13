using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class BienRequerimientosMotivoApertura
{
    public int IdRequerimientoMotivoApertura { get; set; }

    public string Detalle { get; set; } = null!;

    public int IdMotivoApertura { get; set; }

    public bool EsObligatorio { get; set; }

    public bool EsActivo { get; set; }

    public virtual ICollection<BienCasoRequerimiento> BienCasoRequerimientos { get; set; } = new List<BienCasoRequerimiento>();

    public virtual BienMotivoApertura IdMotivoAperturaNavigation { get; set; } = null!;
}
