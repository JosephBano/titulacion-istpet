using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class BienTipoSujeto
{
    public int IdTipoSujeto { get; set; }

    public string Detalle { get; set; } = null!;

    public bool EsActivo { get; set; }

    public virtual ICollection<BienUsuarioCaso> BienUsuarioCasos { get; set; } = [];
}
