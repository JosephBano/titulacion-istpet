using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class BienTipoConvocatorium
{
    public int IdTipoConvocatoria { get; set; }

    public string Detalle { get; set; } = null!;

    public bool EsActivo { get; set; }

    public bool EsInformativo { get; set; }

    public bool Bloquea { get; set; }

    public virtual ICollection<BienConvocatoriasBeca> BienConvocatoriasBecas { get; set; } = new List<BienConvocatoriasBeca>();
}
