using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class BienTipoViviendum
{
    public int IdTipoVivienda { get; set; }

    public string Detalle { get; set; } = null!;

    public virtual ICollection<BienFichaSocioeconomica> BienFichaSocioeconomicas { get; set; } = [];
}
